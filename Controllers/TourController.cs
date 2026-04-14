using backend.Data;
using backend.DTO;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class TourController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly CnpmContext _context;
        private readonly ITourService _tourService;

        public TourController(IJwtService jwtService, CnpmContext cnpmContext, ITourService tourService)
        {
            _context = cnpmContext;
            _jwtService = jwtService;
            _tourService = tourService;
        }

        // ==========================================
        // 1. LẤY DANH SÁCH TOUR (TỰ ĐỘNG NHẬN DIỆN KHÁCH / USER)
        // ==========================================
        [AllowAnonymous]
        [HttpGet("tour")]
        public async Task<IActionResult> GetTour([FromQuery] TourismProductRequest req)
        {
            // Lấy user nếu có đăng nhập, không có thì trả về null
            var user = await TryGetUser();

            // Service của ông đã tự động check: nếu user != null thì buff điểm, null thì không buff
            var paged_result = await _tourService.GetPagedResult(user, req.page, req.pageSize);

            var tourIds = paged_result.Items.Select(t => t.Id).ToList();
            var images = await _context.Imgs.Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId)).ToListAsync();

            var data_result = paged_result.Items.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                title = a.Title,
                description = a.Description,
                durationDays = a.DurationDays,
                numberOfPeople = a.NumberOfPeople,
                price = a.Price,
                vehicle = a.Vehicle,
                tourType = a.TourType,
                status = a.Status,
                departure = new
                {
                    name = a.DepartureLocationName,
                    coords = new[] { a.DepartureLatitude, a.DepartureLongitude }
                },
                rating_average = a.RatingAverage,
                click_count = a.ClickCount,
                favorite_count = a.FavoriteCount,
                trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m), 2),
                type = "tour",
                images = images.Where(img => img.EntityId == a.Id).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return Ok(new
            {
                success = true,
                message = "Thành công",
                data = new
                {
                    items = data_result,
                    totalCount = paged_result.TotalCount,
                    totalPages = paged_result.TotalPages,
                    currentPage = paged_result.CurrentPage
                }
            });
        }

        [AllowAnonymous]
        [HttpGet("detail")]
        public async Task<IActionResult> DetailTour(int id)
        {
            if (id <= 0) return BadRequest("ID không hợp lệ");

            var user = await TryGetUser();
            var tour = await _tourService.GetTourDetail(id);

            // Dù là ai thì cũng tăng ClickCount
            tour.ClickCount += 1;
            bool checkIsFavorite = false;

            // Nếu là User đã đăng nhập thì lưu thêm lịch sử
            if (user != null)
            {
                var history = GetHistoryUser(user);
                if (history.Tour == null) history.Tour = new List<int>();

                UpdateHistoryQueue(history.Tour, id);
                user.User_Search_History = JsonConvert.SerializeObject(history);
                _context.Users.Update(user);

                checkIsFavorite = await _context.Favorites.AnyAsync(f => f.UserId == user.Id && f.EntityId == id && f.EntityType == "tour");
            }

            // Lưu thay đổi (ClickCount và User_History nếu có)
            await _context.SaveChangesAsync();

            var images = await _context.Imgs.Where(img => img.EntityType == "tour" && img.EntityId == id).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Lấy chi tiết Tour thành công",
                data = BuildTourDetailData(tour, images, checkIsFavorite)
            });
        }

        // ==========================================
        // HÀM HELPER ĐÓNG GÓI DỮ LIỆU JSON CHUẨN
        // ==========================================
        private object BuildTourDetailData(Tour tour, List<Img> images, bool isFavorite)
        {
            return new
            {
                id = tour.Id,
                name = tour.Name,
                title = tour.Title,
                description = tour.Description,
                durationDays = tour.DurationDays,
                numberOfPeople = tour.NumberOfPeople,
                price = tour.Price,
                vehicle = tour.Vehicle,
                tourType = tour.TourType,
                status = tour.Status,

                departure = new
                {
                    name = tour.DepartureLocationName,
                    coords = new[] { tour.DepartureLatitude, tour.DepartureLongitude }
                },

                tourist_Area = tour.Tourist_Area != null ? new
                {
                    id = tour.Tourist_Area.Id,
                    name = tour.Tourist_Area.Name
                } : null,

                itineraries = tour.Tour_Itinerarys.OrderBy(ti => ti.DayNumber).Select(ti => new
                {
                    id = ti.Id,
                    dayNumber = ti.DayNumber,
                    activityName = ti.Title,
                    description = ti.Description,
                    tourist_Place = ti.Tourist_Place != null ? new
                    {
                        id = ti.Tourist_Place.Id,
                        name = ti.Tourist_Place.Name,
                        latitude = ti.Tourist_Place.Latitude,
                        longitude = ti.Tourist_Place.Longitude
                    } : null
                }),

                // 🔴 CHỖ NÀY QUAN TRỌNG: Ánh xạ chuẩn cho Form Đặt Tour
                schedules = tour.Departures?.Select(d => new
                {
                    id = d.Id,
                    startDate = d.StartDate,
                    totalSeats = d.TotalSeats,
                    availableSeats = d.AvailableSeats,
                    status = d.Status,
                    // Parse chuỗi JSON từ Database thành mảng string (VD: ["1A", "2B"]) cho Frontend dễ dùng
                    bookedSeats = string.IsNullOrEmpty(d.BookedSeats)
                        ? new List<string>()
                        : JsonConvert.DeserializeObject<List<string>>(d.BookedSeats)
                }).ToList(),

                rating_average = tour.RatingAverage,
                click_count = tour.ClickCount,
                favorite_count = tour.FavoriteCount,
                trending_Score = Math.Round((tour.RatingAverage * 10m) + (tour.FavoriteCount * 2m) + (tour.ClickCount * 0.1m), 2),
                type = "tour",
                isFavorite = isFavorite,

                images = images.ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            };
        }

        // ==========================================
        // HÀM HELPER XỬ LÝ USER & HISTORY
        // ==========================================
        private async Task<User?> TryGetUser()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                // Nếu không có header hoặc không phải Bearer thì trả về null (khách vãng lai)
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return null;

                var oldAccessToken = authHeader.Replace("Bearer ", "");
                var userId = _jwtService.GetUserIdFromToken(oldAccessToken);

                return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch
            {
                // Gặp lỗi (ví dụ token hết hạn, token sai format) thì coi như khách vãng lai
                return null;
            }
        }

        private UserSearchHistory GetHistoryUser(User user)
        {
            if (string.IsNullOrEmpty(user.User_Search_History)) return new UserSearchHistory();
            return JsonConvert.DeserializeObject<UserSearchHistory>(user.User_Search_History) ?? new UserSearchHistory();
        }

        private void UpdateHistoryQueue(List<int> historyList, int newId, int maxItems = 5)
        {
            if (historyList == null) return;
            historyList.Remove(newId);
            historyList.Insert(0, newId);
            if (historyList.Count > maxItems) historyList.RemoveAt(historyList.Count - 1);
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                throw new UnauthorizedException("Vui lòng đăng nhập");
            return userId;
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpPost]
        public async Task<IActionResult> CreateTour([FromBody] TourRequest req)
        {
            int newTourId = await _tourService.CreateTourAsync(req, GetCurrentUserId());
            return Ok(new { success = true, message = "Thêm Tour thành công (Chờ duyệt)", data = new { id = newTourId } });
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTour(int id, [FromBody] TourRequest req)
        {
            await _tourService.UpdateTourAsync(id, req, GetCurrentUserId());
            return Ok(new { success = true, message = "Cập nhật Tour thành công" });
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            await _tourService.DeleteTourAsync(id, GetCurrentUserId());
            return Ok(new { success = true, message = "Xóa Tour thành công" });
        }

        // --- API CHO ITINERARY ---
        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpPost("{tourId:int}/itinerary")] // Route đẹp: /api/Tour/5/itinerary
        public async Task<IActionResult> AddItinerary(int tourId, [FromBody] TourItineraryRequest req)
        {
            await _tourService.AddItineraryAsync(tourId, req, GetCurrentUserId());
            return Ok(new { success = true, message = "Thêm lịch trình thành công" });
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpPut("itinerary/{id:int}")]
        public async Task<IActionResult> UpdateItinerary(int id, [FromBody] TourItineraryRequest req)
        {
            await _tourService.UpdateItineraryAsync(id, req, GetCurrentUserId());
            return Ok(new { success = true, message = "Cập nhật lịch trình thành công" });
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpDelete("itinerary/{id}")]
        public async Task<IActionResult> DeleteItinerary(int id)
        {
            await _tourService.DeleteItineraryAsync(id, GetCurrentUserId());
            return Ok(new { success = true, message = "Xóa lịch trình thành công" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{id:int}")]
        public async Task<IActionResult> ApproveTour(int id, [FromBody] ApprovalRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.Status))
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            await _tourService.ApproveTourAsync(id, req.Status);

            return Ok(new
            {
                success = true,
                message = $"Đã chuyển trạng thái Tour thành: {req.Status}"
            });
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpPost("{tourId}/departures")]
        public async Task<IActionResult> AddDeparture(int tourId, [FromBody] DepartureRequest req)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) return NotFound("Tour không tồn tại");
            if (tour.Created_By_UserId != GetCurrentUserId()) return StatusCode(403, "Đây không phải Tour của bạn");

            var departure = new Tour_Departure
            {
                TourId = tourId,
                StartDate = req.StartDate,
                TotalSeats = req.TotalSeats,
                AvailableSeats = req.TotalSeats,
                Status = "Open"
            };
            _context.TourDepartures.Add(departure);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Mở bán chuyến đi thành công" });
        }

        // ==========================================
        // LẤY DANH SÁCH TOUR CỦA TÔI
        // GET: /api/Tour/my-tours
        // ==========================================
        [Authorize(Roles = "Owner, Tour, Admin, Tour")]
        [HttpGet("my-tours")]
        public async Task<IActionResult> GetMyTours([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] string? status = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var data = await _tourService.GetMyToursAsync(Guid.Parse(userIdStr), page, pageSize, keyword, status);
            return Ok(new { success = true, data = data });
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpPut("departures/{id:int}")]
        public async Task<IActionResult> UpdateDeparture(int id, [FromBody] DepartureRequest req)
        {
            var dep = await _context.TourDepartures.FindAsync(id);
            if (dep == null) return NotFound("Không tìm thấy chuyến đi");

            dep.StartDate = req.StartDate;
            dep.TotalSeats = req.TotalSeats;
            // AvailableSeats có thể tự tính lại dựa trên TotalSeats - số vé đã đặt (tùy logic sếp)
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật chuyến đi thành công" });
        }


        // ==========================================
        // ADMIN: LẤY DANH SÁCH TOUR ĐANG CHỜ DUYỆT
        // GET: /api/Tour/admin/pending
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending")]
        public async Task<IActionResult> GetAllPendingTours([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Tours.Where(t => t.Status == "Pending");

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var tourIds = items.Select(t => t.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId) && img.IsCover)
                    .ToListAsync();

                var dataResult = items.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    title = a.Title,
                    durationDays = a.DurationDays, // Tour có số ngày thay vì địa chỉ
                    rating_average = a.RatingAverage,
                    status = a.Status,
                    coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id)?.url ?? "/Img/ImgNull.jpg"
                });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items = dataResult,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        currentPage = page
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [Authorize(Roles = "Owner, Admin, Tour")]
        [HttpDelete("departures/{id:int}")]
        public async Task<IActionResult> DeleteDeparture(int id)
        {
            var dep = await _context.TourDepartures.FindAsync(id);
            if (dep == null) return NotFound("Không tìm thấy chuyến đi");

            _context.TourDepartures.Remove(dep);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Xóa chuyến đi thành công" });
        }

        // ==========================================
        // ADMIN: LẤY TẤT CẢ TOUR (ĐỂ QUẢN LÝ TỔNG)
        // GET: /api/Tour/admin/all
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllToursForAdmin([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Tours.AsQueryable();

                // Lọc theo từ khóa (Tên tour hoặc Điểm xuất phát)
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(t => t.Name.Contains(keyword) || t.DepartureLocationName.Contains(keyword));
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var tourIds = items.Select(t => t.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId) && img.IsCover)
                    .ToListAsync();

                var dataResult = items.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    title = a.Title,
                    durationDays = a.DurationDays,
                    vehicle = a.Vehicle,
                    price = a.Price,
                    rating_average = a.RatingAverage,
                    status = a.Status,
                    createdAt = a.CreatedAt,
                    createdByUserId = a.Created_By_UserId,
                    coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id)?.url ?? "/Img/ImgNull.jpg"
                });

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items = dataResult,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                        currentPage = page
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}