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

            // Nếu là User đã đăng nhập thì lưu thêm lịch sử
            if (user != null)
            {
                var history = GetHistoryUser(user);
                if (history.Tour == null) history.Tour = new List<int>();

                UpdateHistoryQueue(history.Tour, id);
                user.User_Search_History = JsonConvert.SerializeObject(history);
                _context.Users.Update(user);
            }

            // Lưu thay đổi (ClickCount và User_History nếu có)
            await _context.SaveChangesAsync();

            var images = await _context.Imgs.Where(img => img.EntityType == "tour" && img.EntityId == id).ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Lấy chi tiết Tour thành công",
                data = BuildTourDetailData(tour, images)
            });
        }

        // ==========================================
        // HÀM HELPER ĐÓNG GÓI DỮ LIỆU JSON CHUẨN
        // ==========================================
        private object BuildTourDetailData(Tour tour, List<Img> images)
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
                    // Sửa lại map theo Model Tour_Itinerary hiện tại đang dùng Title
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

                rating_average = tour.RatingAverage,
                click_count = tour.ClickCount,
                favorite_count = tour.FavoriteCount,
                trending_Score = Math.Round((tour.RatingAverage * 10m) + (tour.FavoriteCount * 2m) + (tour.ClickCount * 0.1m), 2),
                type = "tour",

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
    }
}