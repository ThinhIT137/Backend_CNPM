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
            // ==========================================
            // 1. QUERY DATA KHÔNG TRACKING (Tối ưu performance)
            // ==========================================

            // Lấy 10 Tourist Area (Khu du lịch)
            var topTouristAreas = await _context.TouristAreas
                .AsNoTracking()
                .Where(a => a.Status == "Available" || a.Status == "Active")
                .OrderByDescending(a => (a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m))
                .Take(10)
                .ToListAsync();

            // Lấy 10 Tourist Place (Địa điểm du lịch)
            var topTouristPlaces = await _context.TouristPlaces
                .AsNoTracking()
                .Where(p => p.Status == "Available" || p.Status == "Active")
                .OrderByDescending(p => (p.RatingAverage * 10m) + (p.FavoriteCount * 2m) + (p.ClickCount * 0.1m))
                .Take(10)
                .ToListAsync();

            // Lấy 10 Tour
            var topTours = await _context.Tours
                .AsNoTracking()
                .Where(t => t.Status == "Available" || t.Status == "Active")
                .OrderByDescending(t => (t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m))
                .Take(10)
                .ToListAsync();

            // Lấy 10 Hotel (Khách sạn)
            var topHotels = await _context.Hotels
                .AsNoTracking()
                .Where(h => h.Status == "Available" || h.Status == "Active")
                .OrderByDescending(h => (h.RatingAverage * 10m) + (h.FavoriteCount * 2m) + (h.ClickCount * 0.1m))
                .Take(10)
                .ToListAsync();

            // ==========================================
            // 2. LẤY HÌNH ẢNH (Gom ID để tránh N+1 Query)
            // ==========================================
            var touristAreaIds = topTouristAreas.Select(t => t.Id).ToList();
            var touristPlaceIds = topTouristPlaces.Select(p => p.Id).ToList();
            var tourIds = topTours.Select(t => t.Id).ToList();
            var hotelIds = topHotels.Select(h => h.Id).ToList();

            var areaImages = await _context.Imgs.AsNoTracking().Where(img => img.EntityType == "tourist_area" && touristAreaIds.Contains(img.EntityId)).ToListAsync();
            var placeImages = await _context.Imgs.AsNoTracking().Where(img => img.EntityType == "tourist_place" && touristPlaceIds.Contains(img.EntityId)).ToListAsync();
            var tourImages = await _context.Imgs.AsNoTracking().Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId)).ToListAsync();
            var hotelImages = await _context.Imgs.AsNoTracking().Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId)).ToListAsync();

            // ==========================================
            // 3. MAP DỮ LIỆU ĐỂ TRẢ VỀ FRONTEND
            // ==========================================
            var dataResult = new
            {
                touristArea = topTouristAreas.Select(a => new
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
                    images = areaImages.Where(img => img.EntityId == a.Id).ToList(),
                    coverImageUrl = areaImages.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                }),

                touristPlace = topTouristPlaces.Select(a => new
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
                    type = "tourist_place", // Thêm type chuẩn để Front-end dễ dùng
                    images = placeImages.Where(img => img.EntityId == a.Id).ToList(),
                    coverImageUrl = placeImages.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                }),

                tour = topTours.Select(a => new
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
                    images = tourImages.Where(img => img.EntityId == a.Id).ToList(),
                    coverImageUrl = tourImages.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                }),

                hotel = topHotels.Select(a => new
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
                    images = hotelImages.Where(img => img.EntityId == a.Id).ToList(),
                    coverImageUrl = hotelImages.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                })
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
