using backend.DTO;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly CnpmContext _context;
        private readonly ITouristAreaService _touristAreaService;
        private readonly IJwtService _jwtService;

        public HomeController(IConfiguration config, CnpmContext context, ITouristAreaService touristAreaService, IJwtService jwtService)
        {
            _config = config;
            _context = context;
            _touristAreaService = touristAreaService;
            _jwtService = jwtService;
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            var dataTouristArea = await _touristAreaService.GetTrendingTouristAreasAsync(1, 10);
            var touristAreaIds = dataTouristArea.Items.Select(t => t.Id).ToList();
            var images = await _context.Imgs.Where(img => img.EntityType == "tourist_area" && touristAreaIds.Contains(img.EntityId)).ToListAsync();

            var dataTouristPlace = "";
            var dataHottels = "";
            var dataTour = "";

            var dataResult = new
            {
                touristArea = dataTouristArea.Items.Select(a => new
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
                }),
            };


            return Ok(new
            {
                success = true,
                message = "Thành công",
                data = dataResult
            });
        }

        [HttpGet("tour")]
        public async Task<IActionResult> Tours(int tourist_area_id)
        {
            var tours = await _context.Tours.Where(i => i.Tourist_Area_Id == tourist_area_id).ToListAsync();
            var tourIds = tours.Select(t => t.Id).ToList();

            var images = await _context.Imgs.Where(i => i.EntityType == "tour" && tourIds.Contains(i.EntityId)).ToListAsync();

            var tour_Itinerary = await _context.TourItineraries.Include(ti => ti.Tourist_Place).Where(ti => tourIds.Contains(ti.TourId)).ToListAsync();


            var data_result = tours.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                title = a.Title,
                description = a.Description,
                rating_average = a.RatingAverage,
                click_count = a.ClickCount,
                favorite_count = a.FavoriteCount,
                trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m), 2),
                tourItinerary = tour_Itinerary.Where(ti => ti.TourId == a.Id).Select(ti => new
                {
                    id = ti.Id,
                    dayNumber = ti.DayNumber,
                    description = ti.Description,
                    name = ti.Tourist_Place.Name,
                    latitude = ti.Tourist_Place.Latitude,
                    Longitude = ti.Tourist_Place.Longitude,
                }),
                type = "tourist_area",
                images = images.Where(img => img.EntityId == a.Id).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return Ok(new
            {
                success = true,
                message = "Thành Công",
                data = data_result,
            });
        }

        [HttpGet("hottel")]
        public async Task<IActionResult> Hottel(int tourist_place_id)
        {

            var hottels = await _context.Hotels.Where(h => h.Tourist_Place_Id == tourist_place_id).ToListAsync();
            var hottelIDs = hottels.Select(h => h.Id);

            var images = await _context.Imgs.Where(i => i.EntityType == "hottel" && hottelIDs.Contains(i.EntityId)).ToListAsync();
            var data_result = hottels.Select(a => new
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
                data = data_result
            });
        }

        [HttpGet("Tourist_Place")]
        public async Task<IActionResult> Tourist_Place(int tourist_area_id)
        {
            var Tourist_Place = await _context.TouristPlaces.Where(tp => tp.Tourist_Area_Id == tourist_area_id).ToListAsync();
            var images = await _context.Imgs.Where(i => i.EntityType == "tourist_place").ToListAsync();

            var data_result = Tourist_Place.Select(a => new
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
                data = data_result
            });
        }
    }
}
