using backend.DTO;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("/api/[controller]")]
    public class TouristPlaceController : Controller
    {
        private readonly CnpmContext _context;
        private readonly ITouristPlaceService _touristPlaceService;
        private readonly IHotelService _hottelService;
        private readonly ITourService _tourService;

        public TouristPlaceController(CnpmContext context, ITouristPlaceService touristPlaceService, IHotelService hottelService, ITourService tourService)
        {
            _context = context;
            _touristPlaceService = touristPlaceService;
            _hottelService = hottelService;
            _tourService = tourService;
        }

        [HttpGet("tourist_place")]
        public async Task<IActionResult> TouristPlace([FromQuery] TourismProductRequest req) // Tip: GET thì nên dùng FromQuery nha bro
        {
            int page = req.page;
            int pageSize = req.pageSize;

            PagedResult<Tourist_Place> paged_result;

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (currentUser != null)
                {
                    paged_result = await _touristPlaceService.GetPagedResult(currentUser, page, pageSize);
                }
                else
                {
                    paged_result = await _touristPlaceService.GetPagedResult(page, pageSize);
                }
            }
            else
            {
                paged_result = await _touristPlaceService.GetPagedResult(page, pageSize);
            }

            // 4. MAP DATA VÀ HÌNH ẢNH (Giữ nguyên logic cũ của ông)
            var touristPlaceIds = paged_result.Items.Select(t => t.Id).ToList();

            // Note: Chỗ này EntityType của ông đang để "tourist_area", check kỹ xem DB là tourist_area hay tourist_place nha
            var images = await _context.Imgs
                .Where(img => img.EntityType == "tourist_place" && touristPlaceIds.Contains(img.EntityId))
                .ToListAsync();

            var data_result = paged_result.Items.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                title = a.Title,
                address = a.Address,
                description = a.Description,
                rating_average = a.RatingAverage,
                click_count = a.ClickCount,
                favorite_count = a.FavoriteCount,
                trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m), 2),
                latitude = a.Latitude,
                longitude = a.Longitude,
                type = "tourist_area",
                images = images.Where(img => img.EntityId == a.Id).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            Console.WriteLine("page :" + paged_result.TotalPages);

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

        [HttpPost("detail")]
        public async Task<IActionResult> GetTouristPlaceDetail([FromBody] TourismProductDetailRequest req)
        {
            Console.WriteLine("goi api");
            int id = req.id;
            string type = req.type;
            int page = req.TourismProduct?.page ?? 1;
            int pageSize = req.TourismProduct?.pageSize ?? 10;

            // 🔴 2. LẤY USER AN TOÀN CHO CẢ GUEST LẪN NGƯỜI ĐĂNG NHẬP
            User? currentUser = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid parsedId))
                {
                    currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedId);
                }
            }

            var touristPlace = await _context.TouristPlaces.FirstOrDefaultAsync(tp => tp.Id == id);
            if (touristPlace == null) throw new NotFoundException("Không tìm thấy địa điểm");

            var placeImages = await _context.Imgs
                .Where(img => img.EntityType == "tourist_place" && img.EntityId == id)
                .ToListAsync();

            // 🔴 3. KIỂM TRA THẢ TIM
            bool checkIsFavorite = false;
            if (currentUser != null)
            {
                checkIsFavorite = await _context.Favorites.AnyAsync(i => i.UserId == currentUser.Id && i.EntityId == touristPlace.Id && i.EntityType == "tourist_place");
            }

            var touristPlaceDetail = new
            {
                id = touristPlace.Id,
                name = touristPlace.Name,
                title = touristPlace.Title,
                address = touristPlace.Address,
                description = touristPlace.Description,
                latitude = touristPlace.Latitude,
                longitude = touristPlace.Longitude,
                rating_average = touristPlace.RatingAverage,
                favorite_count = touristPlace.FavoriteCount,
                click_count = touristPlace.ClickCount,
                isFavorite = checkIsFavorite, // 🔴 4. ĐÃ NHÉT BIẾN NÀY VÀO ĐỂ FRONTEND NHẬN ĐƯỢC
                images = placeImages.ToList(),
                coverImageUrl = placeImages.FirstOrDefault(img => img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            };

            if (type == "Hotel")
            {
                var paged_result = await _hottelService.GetHotelsByTouristPlaceId(id, currentUser, page, pageSize);

                var hotelIds = paged_result.Items.Select(h => h.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId))
                    .ToListAsync();

                var data_result = paged_result.Items.Select(a => new
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

                return Ok(new
                {
                    success = true,
                    tourist_Place_Detail = touristPlaceDetail,
                    pagedResult = new
                    {
                        items = data_result,
                        totalCount = paged_result.TotalCount,
                        totalPages = paged_result.TotalPages,
                        currentPage = paged_result.CurrentPage
                    }
                });
            }

            // 2. TRẢ VỀ DỮ LIỆU TOUR
            else if (type == "Tour")
            {
                var paged_result = await _tourService.GetToursByTouristPlaceId(id, currentUser, page, pageSize);

                var tourIds = paged_result.Items.Select(t => t.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId))
                    .ToListAsync();

                var data_result = paged_result.Items.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    title = a.Title,
                    description = a.Description,
                    durationDays = a.DurationDays,
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
                    tourist_Place_Detail = touristPlaceDetail,
                    pagedResult = new
                    {
                        items = data_result,
                        totalCount = paged_result.TotalCount,
                        totalPages = paged_result.TotalPages,
                        currentPage = paged_result.CurrentPage
                    }
                });
            }

            throw new BadRequestException("Type không hợp lệ");
        }

        // ==========================================
        // LẤY DANH SÁCH ĐỊA ĐIỂM DU LỊCH CỦA TÔI
        // GET: /api/TouristPlace/my-places
        // ==========================================
        [Authorize(Roles = "Admin, Owner, Hotel, Tour, User")]
        [HttpGet("my-places")]
        public async Task<IActionResult> GetMyTouristPlaces([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] string? status = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var data = await _touristPlaceService.GetMyTouristPlacesAsync(Guid.Parse(userIdStr), page, pageSize, keyword, status);
            return Ok(new { success = true, data = data });
        }

        [Authorize(Roles = "Owner, Admin, User, Hotel, Tour")]
        [HttpPost]
        public async Task<IActionResult> CreateTouristPlace([FromBody] TouristPlaceRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int newId = await _touristPlaceService.CreateTouristPlaceAsync(req, userId);
            return Ok(new
            {
                success = true,
                message = "Thêm địa điểm thành công",
                data = new { id = newId }
            });
        }

        [Authorize(Roles = "Owner, Admin, User, Hotel, Tour")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTouristPlace(int id, [FromBody] TouristPlaceRequest req)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _touristPlaceService.UpdateTouristPlaceAsync(id, req, userId);
            return Ok(new { success = true, message = "Cập nhật địa điểm thành công" });
        }

        [Authorize(Roles = "Owner, Admin, User, Hotel, Tour")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTouristPlace(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _touristPlaceService.DeleteTouristPlaceAsync(id, userId);
            return Ok(new { success = true, message = "Xóa địa điểm thành công" });
        }

        [AllowAnonymous]
        [HttpGet("all-dropdown")]
        public async Task<IActionResult> GetAllForDropdown()
        {
            var data = await _touristPlaceService.GetAllForDropdownAsync();
            return Ok(new { success = true, data = data });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{id:int}")]
        public async Task<IActionResult> ApproveTouristPlace(int id, [FromBody] ApprovalRequest req)
        {
            var place = await _context.TouristPlaces.FindAsync(id);
            if (place == null) return NotFound(new { success = false, message = "Không tìm thấy địa điểm" });

            place.Status = req.Status;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã duyệt địa điểm thành {req.Status}" });
        }

        // ==========================================
        // ADMIN: LẤY DANH SÁCH ĐỊA ĐIỂM ĐANG CHỜ DUYỆT
        // GET: /api/TouristPlace/admin/pending
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending")]
        public async Task<IActionResult> GetAllPendingTouristPlaces([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.TouristPlaces.Where(p => p.Status == "Pending");

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var placeIds = items.Select(p => p.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "tourist_place" && placeIds.Contains(img.EntityId) && img.IsCover)
                    .ToListAsync();

                var dataResult = items.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    title = a.Title,
                    address = a.Address,
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
    }
}

