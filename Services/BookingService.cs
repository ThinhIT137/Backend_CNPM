using backend.DTO;
using backend.Exceptions;
using backend.Hubs;
using backend.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Services
{
    public class BookingService : IBookingService
    {
        private readonly CnpmContext _context;
        private readonly IEmailService _emailService;
        private readonly IHubContext<NotificationHub> _hubContext; // 🔴 Thêm HubContext

        public BookingService(CnpmContext context, IEmailService emailService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        public async Task CreateBookingAsync(Guid userId, BookingRequest req)
        {
            // 1. TẠO HÓA ĐƠN GỐC (BOOKING)
            var booking = new Booking
            {
                UserId = userId,
                BookingType = req.BookingType,
                ContactName = req.ContactName,
                ContactPhone = req.ContactPhone,
                Note = req.Note,
                BookingStatus = "Pending",
                PaymentStatus = "Unpaid",
                CreatedAt = DateTime.Now
            };

            decimal totalAmount = 0;
            var details = new List<Booking_Detail>();

            // ===============================================
            // 2. XỬ LÝ NẾU KHÁCH ĐẶT KHÁCH SẠN
            // ===============================================
            if (req.BookingType.Equals("Hotel", StringComparison.OrdinalIgnoreCase))
            {
                if (req.HotelRoomIds == null || !req.HotelRoomIds.Any())
                    throw new BadRequestException("Vui lòng chọn ít nhất 1 phòng!");

                foreach (var roomId in req.HotelRoomIds)
                {
                    var room = await _context.HotelRooms.FindAsync(roomId);
                    if (room == null) throw new NotFoundException($"Không tìm thấy phòng có ID {roomId}");

                    if (room.Status != "Available")
                        throw new BadRequestException($"Phòng {room.RoomName} đã có người đặt!");

                    // Khóa phòng lại
                    room.Status = "Booked";

                    // Tính tiền
                    totalAmount += room.Price;

                    // Tạo chi tiết hóa đơn
                    details.Add(new Booking_Detail
                    {
                        HotelRoomId = roomId,
                        UnitPrice = room.Price
                    });
                }
            }
            // ===============================================
            // 3. XỬ LÝ NẾU KHÁCH ĐẶT TOUR
            // ===============================================
            else if (req.BookingType.Equals("Tour", StringComparison.OrdinalIgnoreCase))
            {
                if (req.TourDepartureId == null) throw new BadRequestException("Vui lòng chọn chuyến đi!");

                var departure = await _context.TourDepartures
                    .Include(d => d.Tour).AsNoTracking().AsSplitQuery()
                    .FirstOrDefaultAsync(d => d.Id == req.TourDepartureId);

                if (departure == null) throw new NotFoundException("Chuyến đi không tồn tại");
                if (departure.Status != "Open") throw new BadRequestException("Chuyến đi này đã đóng hoặc đã đầy!");

                decimal unitPrice = departure.Tour?.Price ?? 0;

                // TRƯỜNG HỢP 3.1: BAO NGUYÊN XE
                if (req.IsPrivateTour)
                {
                    if (departure.AvailableSeats < departure.TotalSeats)
                        throw new BadRequestException("Chuyến này đã có người đặt ghép, không thể bao nguyên xe!");

                    totalAmount = unitPrice * departure.TotalSeats; // Hoặc một mức giá deal riêng
                    departure.AvailableSeats = 0;
                    departure.Status = "Full";

                    details.Add(new Booking_Detail
                    {
                        TourDepartureId = departure.Id,
                        IsPrivateTour = true,
                        UnitPrice = totalAmount
                    });
                }
                // TRƯỜNG HỢP 3.2: ĐẶT GHẾ GHÉP
                else
                {
                    if (req.SeatNumbers == null || !req.SeatNumbers.Any())
                        throw new BadRequestException("Vui lòng chọn ít nhất 1 ghế!");

                    if (departure.AvailableSeats < req.SeatNumbers.Count)
                        throw new BadRequestException("Không đủ ghế trống!");

                    // Lấy danh sách ghế đã bị đặt từ JSON trong DB ra
                    var currentBookedSeats = string.IsNullOrEmpty(departure.BookedSeats)
                        ? new List<string>()
                        : JsonConvert.DeserializeObject<List<string>>(departure.BookedSeats);

                    // Quét xem có ghế nào khách vừa chọn mà nằm trong danh sách đã đặt không
                    foreach (var seat in req.SeatNumbers)
                    {
                        if (currentBookedSeats!.Contains(seat))
                            throw new BadRequestException($"Ghế {seat} đã bị người khác nhanh tay đặt mất! Vui lòng chọn ghế khác.");
                    }

                    // Cập nhật lại số lượng và mảng ghế vào DB
                    currentBookedSeats!.AddRange(req.SeatNumbers);
                    departure.BookedSeats = JsonConvert.SerializeObject(currentBookedSeats);
                    departure.AvailableSeats -= req.SeatNumbers.Count;

                    if (departure.AvailableSeats == 0) departure.Status = "Full";

                    // Tính tiền
                    totalAmount = unitPrice * req.SeatNumbers.Count;

                    foreach (var seat in req.SeatNumbers)
                    {
                        details.Add(new Booking_Detail
                        {
                            TourDepartureId = departure.Id,
                            SeatNumber = seat,
                            UnitPrice = unitPrice
                        });
                    }
                }
            }
            else
            {
                throw new BadRequestException("Loại Booking không hợp lệ!");
            }

            Guid? ownerId = null;
            string productName = "";

            if (req.BookingType == "Hotel" && details.Any())
            {
                var firstRoom = await _context.HotelRooms.Include(r => r.Hotel).AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(r => r.Id == details.First().HotelRoomId);
                ownerId = firstRoom?.Hotel?.Created_By_UserId;
                productName = firstRoom?.Hotel?.Name ?? "Khách sạn";
            }
            else if (req.BookingType == "Tour" && details.Any())
            {
                var firstDep = await _context.TourDepartures.Include(d => d.Tour).AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(d => d.Id == details.First().TourDepartureId);
                ownerId = firstDep?.Tour?.Created_By_UserId;
                productName = firstDep?.Tour?.Name ?? "Tour";
            }

            if (ownerId.HasValue)
            {
                var owner = await _context.Users.FindAsync(ownerId.Value);
                if (owner != null)
                {
                    // Ghi vào bảng Notification (Cái chuông trên App)
                    _context.Notifications.Add(new Notification
                    {
                        UserId = owner.Id,
                        Title = "🎉 Có khách đặt mới!",
                        Content = $"Khách hàng {req.ContactName} vừa đặt {productName}. Mã đơn: #{booking.Id}",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    // Bắn Email cho Owner (Khúc này húp điểm nè)
                    string emailSubject = $"[App Du Lịch] Thông báo đơn đặt mới #{booking.Id}";
                    string emailBody = $"<h3>Chào {owner.Name},</h3>" +
                                       $"<p>Bạn vừa nhận được một đơn đặt mới từ khách hàng <b>{req.ContactName}</b> (SĐT: {req.ContactPhone}).</p>" +
                                       $"<p>Sản phẩm: {productName}</p>" +
                                       $"<p>Tổng tiền: <b>{totalAmount:N0} VNĐ</b></p>" +
                                       $"<p>Vui lòng đăng nhập vào hệ thống để xác nhận đơn hàng.</p>";

                    // Không dùng await ở đây để tránh làm chậm luồng của user, cho nó chạy ngầm
                    _ = _emailService.SendEmailAsync(owner, emailSubject, emailBody);
                }
            }

            // 4. LƯU TẤT CẢ VÀO DATABASE
            booking.TotalAmount = totalAmount;
            booking.BookingDetails = details;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            if (ownerId.HasValue)
            {
                var notification = new Notification
                {
                    UserId = ownerId.Value,
                    Title = "🎉 Đơn đặt mới!",
                    Content = $"Khách {req.ContactName} vừa đặt {productName}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // 🔴 BẮN REAL-TIME Ở ĐÂY
                // Gửi cho đúng người sở hữu (dựa vào UserId của họ)
                await _hubContext.Clients.User(ownerId.Value.ToString()).SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    content = notification.Content,
                    createdAt = notification.CreatedAt,
                    isRead = false
                });
            }

            var customerNotification = new Notification
            {
                UserId = userId,
                Title = "✅ Đặt chỗ thành công!",
                Content = $"Đơn đặt {productName} của bạn đã được ghi nhận và đang chờ xác nhận. Mã đơn: #{booking.Id}",
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(customerNotification);
            await _context.SaveChangesAsync();

            // Bắn SignalR kêu "ting ting" cho Khách
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
            {
                id = customerNotification.Id,
                title = customerNotification.Title,
                content = customerNotification.Content,
                createdAt = customerNotification.CreatedAt,
                isRead = false
            });
        }
    }
}