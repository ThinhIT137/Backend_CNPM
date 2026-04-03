using backend.DTO;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("/api/[controller]")]
    public class TouristPlaceController : Controller
    {
        private readonly CnpmContext _context;
        private readonly ITouristPlaceService _touristPlaceService;
        private readonly IHottelService _hottelService;
        private readonly ITourService _tourService;

        public TouristPlaceController(CnpmContext context, ITouristPlaceService touristPlaceService, IHottelService hottelService, ITourService tourService)
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
            var touristAreaIds = paged_result.Items.Select(t => t.Id).ToList();

            // Note: Chỗ này EntityType của ông đang để "tourist_area", check kỹ xem DB là tourist_area hay tourist_place nha
            var images = await _context.Imgs
                .Where(img => img.EntityType == "tourist_area" && touristAreaIds.Contains(img.EntityId))
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

            User? user = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid parsedId))
                {
                    user = await _context.Users.Where(u => u.Id == parsedId).FirstOrDefaultAsync();
                }
            }

            var touristPlaceDetail = await _context.TouristPlaces
                .Select(tp => new
                {
                    tp.Id,
                    tp.Name,
                    tp.Latitude,
                    tp.Longitude,
                    tp.Description
                })
                .FirstOrDefaultAsync(tp => tp.Id == id);

            if (touristPlaceDetail == null) return NotFound(new { success = false, message = "Không tìm thấy địa điểm" });

            if (type == "Hotel")
            {
                var paged_result = await _hottelService.GetHotelsByTouristPlaceId(id, user, page, pageSize);

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
                var paged_result = await _tourService.GetToursByTouristPlaceId(id, user, page, pageSize);

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
    }
}

