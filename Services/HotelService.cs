using backend.Data;
using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace backend.Services
{
    public class HotelService : IHotelService
    {
        public CnpmContext _context { get; set; }
        public HotelService(CnpmContext context)
        {
            _context = context;
        }

        public async Task<object> GetTrendingHottelAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Hotels.Where(h => h.Status == "Active").AsQueryable();

            var totalCount = await query.CountAsync();

            var hotels = await query
                .OrderByDescending(h => (h.RatingAverage * 10m) + (h.FavoriteCount * 2m) + (h.ClickCount * 0.1m))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var hotelIds = hotels.Select(h => h.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = hotels.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                title = a.Title,
                address = a.Address,
                description = a.Description,
                price = a.Price,
                rating_average = a.RatingAverage,
                click_count = a.ClickCount,
                favorite_count = a.FavoriteCount,
                trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m), 2),
                latitude = a.Latitude,
                longitude = a.Longitude,
                type = "hotel",
                images = images.Where(img => img.EntityId == a.Id).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return new
            {
                items = dataResult,
                totalCount = totalCount,
                totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page
            };
        }

        public async Task<object> GetTrendingHottelAsync(User u, int page = 1, int pageSize = 10)
        {
            var query = _context.Hotels.Where(h => h.Status == "Active").AsQueryable();
            var totalCount = await query.CountAsync();

            var history = GetHistoryUser(u);
            var recentHotels = history.Hottel ?? new List<int>();

            var hotels = await query
                .OrderByDescending(h =>
                    (h.RatingAverage * 10m) +
                    (h.FavoriteCount * 2m) +
                    (h.ClickCount * 0.1m) +
                    (recentHotels.Contains(h.Id) ? 50m : 0m)
                )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var hotelIds = hotels.Select(h => h.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = hotels.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                title = a.Title,
                address = a.Address,
                description = a.Description,
                price = a.Price,
                rating_average = a.RatingAverage,
                click_count = a.ClickCount,
                favorite_count = a.FavoriteCount,
                trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m) + (recentHotels.Contains(a.Id) ? 50m : 0m), 2),
                latitude = a.Latitude,
                longitude = a.Longitude,
                type = "hotel",
                images = images.Where(img => img.EntityId == a.Id).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return new
            {
                items = dataResult,
                totalCount = totalCount,
                totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page
            };
        }

        public async Task<PagedResult<Hotel>> GetHotelsByTouristPlaceId(int touristPlaceId, User? user, int page = 1, int pageSize = 10)
        {
            var query = _context.Hotels.Where(h => h.Tourist_Place_Id == touristPlaceId && h.Status == "Active").AsNoTracking().AsSplitQuery();
            var TotalCount = await query.CountAsync();

            var historyList = new List<int>();
            if (user != null)
            {
                var history = GetHistoryUser(user);
                historyList = history.Hottel ?? new List<int>();
            }

            var data = await query.OrderByDescending(h => ((h.RatingAverage * 10m) + (h.FavoriteCount * 2m) + (h.ClickCount * 0.1m) + (historyList.Contains(h.Id) ? 50m : 0m)))
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            //if (data == null || data.Count == 0)
            //{
            //    throw new BadRequestException("Không có khách sạn nào gần địa điểm này");
            //}

            return new PagedResult<Hotel>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task<object> GetHotelDetailAsync(int id, User? user)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Tourist_Place)
                .Include(h => h.Rooms)
                .AsNoTracking().AsSplitQuery()
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
            {
                throw new BadRequestException("Không tìm thấy khách sạn này");
            }

            hotel.ClickCount += 1;

            if (user != null)
            {
                var history = GetHistoryUser(user);
                if (history.Hottel == null) history.Hottel = new List<int>();

                UpdateHistoryQueue(history.Hottel, id);

                user.User_Search_History = JsonConvert.SerializeObject(history);
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            var images = await _context.Imgs
                .Where(img => img.EntityType == "hotel" && img.EntityId == id)
                .AsNoTracking().AsSplitQuery()
                .ToListAsync();

            return new
            {
                id = hotel.Id,
                name = hotel.Name,
                title = hotel.Title,
                address = hotel.Address,
                description = hotel.Description,
                price = hotel.Price,
                latitude = hotel.Latitude,
                longitude = hotel.Longitude,
                status = hotel.Status,
                number_of_people = hotel.NumberOfPeople,
                rating_average = hotel.RatingAverage,
                rating_count = hotel.RatingCount,
                click_count = hotel.ClickCount,
                favorite_count = hotel.FavoriteCount,
                trending_Score = Math.Round((hotel.RatingAverage * 10m) + (hotel.FavoriteCount * 2m) + (hotel.ClickCount * 0.1m), 2),
                tourist_place = hotel.Tourist_Place != null ? new
                {
                    id = hotel.Tourist_Place.Id,
                    name = hotel.Tourist_Place.Name
                } : null,

                // 🔴 CHỖ NÀY QUAN TRỌNG: Lấy đủ thông tin cho Form Đặt Phòng
                rooms = hotel.Rooms.Select(r => new
                {
                    id = r.Id,
                    roomName = r.RoomName,
                    floor = r.Floor, // Phải có tầng để Frontend gom nhóm
                    roomType = r.RoomType,
                    price = r.Price,
                    status = r.Status // Phải có Status ('Available', 'Booked') để tô màu xám/xanh
                }).ToList(),

                images = images,
                coverImageUrl = images.FirstOrDefault(img => img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            };
        }

        // THÊM KHÁCH SẠN
        public async Task<int> CreateHotelAsync(HotelRequest req, Guid ownerId)
        {
            var hotel = new Hotel
            {
                Name = req.Name,
                Address = req.Address,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Description = req.Description,
                Title = req.Title,
                NumberOfPeople = req.NumberOfPeople,
                Tourist_Place_Id = req.Tourist_Place_Id,
                Price = req.Price,
                Created_By_UserId = ownerId,
                Status = "Approved", // QUAN TRỌNG: Mới tạo thì phải chờ Admin duyệt
                CreatedAt = DateTime.Now
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();
            return hotel.Id;
        }

        // SỬA KHÁCH SẠN
        public async Task UpdateHotelAsync(int id, HotelRequest req, Guid ownerId)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null) throw new NotFoundException("Không tìm thấy khách sạn");

            // CHECK QUYỀN CHÍNH CHỦ
            if (hotel.Created_By_UserId != ownerId)
                throw new ForbiddenException("Bạn không có quyền sửa khách sạn của người khác!");

            hotel.Name = req.Name;
            hotel.Address = req.Address;
            hotel.Latitude = req.Latitude;
            hotel.Longitude = req.Longitude;
            hotel.Description = req.Description;
            hotel.Title = req.Title;
            hotel.NumberOfPeople = req.NumberOfPeople;
            hotel.Tourist_Place_Id = req.Tourist_Place_Id;
            hotel.Price = req.Price;
            hotel.Status = "Pending"; // Sửa xong bắt Admin duyệt lại cho chắc cú

            _context.Hotels.Update(hotel);
            await _context.SaveChangesAsync();
        }

        // XÓA KHÁCH SẠN
        public async Task DeleteHotelAsync(int id, Guid ownerId)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null) throw new NotFoundException("Không tìm thấy khách sạn");

            // CHECK QUYỀN CHÍNH CHỦ
            if (hotel.Created_By_UserId != ownerId)
                throw new ForbiddenException("Bạn không có quyền xóa khách sạn của người khác!");

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
        }

        public async Task ApproveHotelAsync(int id, string status)
        {
            var validStatuses = new[] { "Approved", "Rejected", "Pending" };
            if (!validStatuses.Contains(status))
                throw new BadRequestException("Trạng thái không hợp lệ. Chỉ nhận: Approved, Rejected, Pending");

            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                throw new NotFoundException("Không tìm thấy khách sạn này");

            hotel.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task<object> GetMyHotelsAsync(Guid userId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null)
        {
            // 1. LỌC THÔ BẰNG SQL (Lấy đúng hàng của User này)
            var query = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Tourist_Place)
                .Where(h => h.Created_By_UserId == userId)
                .AsNoTracking()
                .AsSplitQuery();

            // Nếu người dùng chọn lọc trạng thái (Active/Pending/Rejected)
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(h => h.Status == status);
            }

            // Kéo data lên RAM (Vì data 1 user ít nên thoải mái)
            var allMyHotels = await query.ToListAsync();

            // 2. LỌC TINH (SMART SEARCH) BẰNG STRING HELPER
            if (!string.IsNullOrEmpty(keyword))
            {
                string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower().Trim();

                var scoredHotels = allMyHotels.Select(h =>
                {
                    string unSignName = StringHelper.ConvertToUnSign(h.Name ?? "").ToLower();
                    string unSignAddress = StringHelper.ConvertToUnSign(h.Address ?? "").ToLower();

                    // Ưu tiên tìm theo Tên (x3), sau đó tới Địa chỉ (x1.5)
                    double maxSimilarity = new[] {
                        StringHelper.CalculateSimilarity(unSignKeyword, unSignName) * 3.0,
                        StringHelper.CalculateSimilarity(unSignKeyword, unSignAddress) * 1.5
                    }.Max();

                    // Thưởng điểm nếu gõ đúng 1 phần của chuỗi
                    if (unSignName.Contains(unSignKeyword) || unSignAddress.Contains(unSignKeyword))
                    {
                        maxSimilarity += 5.0;
                    }

                    return new { Item = h, Score = maxSimilarity };
                })
                .Where(x => x.Score >= 0.8) // Điểm chuẩn để được lọt vào danh sách
                .OrderByDescending(x => x.Score) // Giống nhất xếp trên cùng
                .ThenByDescending(x => x.Item.CreatedAt)
                .ToList();

                // Gán lại danh sách đã lọc bằng Smart Search
                allMyHotels = scoredHotels.Select(x => x.Item).ToList();
            }
            else
            {
                // Nếu không gõ gì thì xếp mới nhất lên đầu
                allMyHotels = allMyHotels.OrderByDescending(h => h.CreatedAt).ToList();
            }

            // 3. XỬ LÝ PHÂN TRANG VÀ HÌNH ẢNH TRÊN KẾT QUẢ ĐÃ LỌC
            var totalCount = allMyHotels.Count;
            var pagedHotels = allMyHotels.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var hotelIds = pagedHotels.Select(h => h.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = pagedHotels.Select(h => new
            {
                id = h.Id,
                name = h.Name,
                title = h.Title,
                address = h.Address,
                description = h.Description,
                latitude = h.Latitude,
                longitude = h.Longitude,
                price = h.Price,
                status = h.Status,
                numberOfPeople = h.NumberOfPeople,
                tourist_Place_Id = h.Tourist_Place_Id,
                tourist_Area_Id = h.Tourist_Place?.Tourist_Area_Id,

                rooms = h.Rooms.Select(r => new
                {
                    id = r.Id,
                    roomName = r.RoomName,
                    floor = r.Floor,
                    roomType = r.RoomType,
                    price = r.Price
                }).ToList(),

                images = images.Where(img => img.EntityId == h.Id).Select(img => new
                {
                    id = img.Id,
                    url = img.url,
                    isCover = img.IsCover
                }).ToList(),

                coverImageUrl = images.FirstOrDefault(img => img.EntityId == h.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return new
            {
                items = dataResult,
                totalCount = totalCount,
                totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page
            };
        }

        private UserSearchHistory GetHistoryUser(User user)
        {
            if (string.IsNullOrEmpty(user.User_Search_History))
            {
                return new UserSearchHistory();
            }
            return JsonConvert.DeserializeObject<UserSearchHistory>(user.User_Search_History) ?? new UserSearchHistory();
        }

        private void UpdateHistoryQueue(List<int> historyList, int newId, int maxItems = 5)
        {
            if (historyList == null) return;
            historyList.Remove(newId);
            historyList.Insert(0, newId);
            if (historyList.Count > maxItems)
            {
                historyList.RemoveAt(historyList.Count - 1);
            }
        }
    }
}