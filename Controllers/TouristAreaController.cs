using backend.DTO;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("/api/[controller]")]
    public class TouristAreaController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly CnpmContext _context;
        private readonly ITouristAreaService _touristAreaService;

        public TouristAreaController(IJwtService jwtService, CnpmContext cnpmContext, ITouristAreaService touristAreaService)
        {
            _context = cnpmContext;
            _jwtService = jwtService;
            _touristAreaService = touristAreaService;
        }

        [Authorize]
        [HttpGet("tourist_area_user")]
        public async Task<IActionResult> tourist_area_user(int page, int pageSize)
        {
            // lấy accessToken từ Authorization
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                throw new SecurityTokenException("Không tìm thấy Access Token cũ");
            // đọc token trả về UserId
            var oldAccessToken = authHeader.Replace("Bearer ", "");
            var userId = _jwtService.GetUserIdFromToken(oldAccessToken);

            User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new SecurityTokenException("User không tồn tại");

            var paged_result = await _touristAreaService.GetTrendingTouristAreasAsync(user, page, pageSize);
            var touristAreaIds = paged_result.Items.Select(t => t.Id).ToList();
            var images = await _context.Imgs.Where(img => img.EntityType == "tourist_area" && touristAreaIds.Contains(img.EntityId)).ToListAsync();

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
        [HttpGet("tourist_area")]
        public async Task<IActionResult> tourist_area([FromQuery] TouristAreaRequest req)
        {
            int page = req.page;
            int pageSize = req.pageSize;
            var paged_result = await _touristAreaService.GetTrendingTouristAreasAsync(page, pageSize);
            var touristAreaIds = paged_result.Items.Select(t => t.Id).ToList();
            var images = await _context.Imgs.Where(img => img.EntityType == "tourist_area" && touristAreaIds.Contains(img.EntityId)).ToListAsync();

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

        [AllowAnonymous]
        [HttpGet("DetailTouristArea")]
        public async Task<IActionResult> DetailTouristArea([FromBody] TouristAreaDetailRequest req)
        {
            var data_result = await _touristAreaService.GetDetailTouristAreasAsync(req.id);

            return Ok(new
            {
                success = true,
                message = "Thành công",
                data = data_result
            });
        }
    }
}
