using backend.DTO;
using backend.Exceptions;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

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
        public async Task<IActionResult> tourist_area_user([FromQuery] TourismProductRequest req)
        {
            var user = await getUser();
            var paged_result = await _touristAreaService.GetTrendingTouristAreasAsync(user, req.page, req.pageSize);
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
        public async Task<IActionResult> tourist_area([FromQuery] TourismProductRequest req)
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
        [HttpPost("DetailTouristArea")]
        public async Task<IActionResult> DetailTouristArea([FromBody] TourismProductDetailRequest req)
        {

            if (req == null) return BadRequest("Không nhận được dữ liệu");

            if (req.TourismProduct == null)
            {
                Console.WriteLine("Ở đây NULLLLLLLLLLLLLLLLLLLLLLLLLLL");
                throw new BadRequestException("Đối tượng TourismProduct bị null. Hãy check lại chữ hoa/chữ thường trong JSON!");
            }

            var data = await _touristAreaService.GetDetailTouristAreasAsync(req.id, req.type, req.TourismProduct.page, req.TourismProduct.pageSize);
            var touristArea = data.tourist_Area_Detail;
            var images = await _context.Imgs.Where(img => img.EntityType == "tourist_area" && touristArea.Id == img.EntityId).ToListAsync();
            if (req.type == "TouristPlace")
            {
                var tourist_Place = data.TouristPlaces.Items;
                var touristPlaceIds = tourist_Place.Select(t => t.Id).ToList();
                var imagesTouristPlace = await _context.Imgs.Where(img => img.EntityType == "tourist_place" && touristPlaceIds.Contains(img.EntityId)).ToListAsync();
                var totalCountTouristPlace = tourist_Place.Count();
                var data_result_Tourist_Place = new
                {
                    items = tourist_Place.Select(a => new
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
                        type = "tourist_place",
                        images = imagesTouristPlace.Where(img => img.EntityId == a.Id).ToList(),
                        coverImageUrl = imagesTouristPlace.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                    }),
                    totalCount = totalCountTouristPlace,
                    totalPages = (int)Math.Ceiling(totalCountTouristPlace / (double)req.TourismProduct.pageSize),
                    currentPage = req.TourismProduct.page
                };

                return Ok(new
                {
                    success = true,
                    message = "Thành công",
                    data = new
                    {
                        tourist_Area_Detail = new
                        {
                            id = touristArea.Id,
                            name = touristArea.Name,
                            title = touristArea.Title,
                            address = touristArea.Address,
                            description = touristArea.Description,
                            rating_average = touristArea.RatingAverage,
                            click_count = touristArea.ClickCount,
                            favorite_count = touristArea.FavoriteCount,
                            trending_Score = Math.Round((touristArea.RatingAverage * 10m) + (touristArea.FavoriteCount * 2m) + (touristArea.ClickCount * 0.1m), 2),
                            latitude = touristArea.Latitude,
                            longitude = touristArea.Longitude,
                            type = "tourist_area",
                            images = images.ToList(),
                            coverImageUrl = images.FirstOrDefault(img => img.EntityId == touristArea.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                        },
                        pagedResult = data_result_Tourist_Place
                    }
                });
            }

            var tour = data.Tours.Items;
            var tourIds = tour.Select(t => t.Id).ToList();
            var imagesTour = await _context.Imgs.Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId)).ToListAsync();
            var totalCount = tour.Count();
            var data_result_Tour = new
            {
                items = tour.Select(a => new
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
                    images = imagesTour.Where(img => img.EntityId == a.Id).ToList(),
                    coverImageUrl = imagesTour.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                }),
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)req.TourismProduct.pageSize),
                currentPage = req.TourismProduct.page
            };

            return Ok(new
            {
                success = true,
                message = "Thành công",
                data = new
                {
                    tourist_Area_Detail = new
                    {
                        id = touristArea.Id,
                        name = touristArea.Name,
                        title = touristArea.Title,
                        address = touristArea.Address,
                        description = touristArea.Description,
                        rating_average = touristArea.RatingAverage,
                        click_count = touristArea.ClickCount,
                        favorite_count = touristArea.FavoriteCount,
                        trending_Score = Math.Round((touristArea.RatingAverage * 10m) + (touristArea.FavoriteCount * 2m) + (touristArea.ClickCount * 0.1m), 2),
                        latitude = touristArea.Latitude,
                        longitude = touristArea.Longitude,
                        type = "tourist_area",
                        images = images.ToList(),
                        coverImageUrl = images.FirstOrDefault(img => img.EntityId == touristArea.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                    },
                    pagedResult = data_result_Tour
                }
            });
        }


        [AllowAnonymous]
        [HttpPost("DetailTouristAreaUser")]
        public async Task<IActionResult> DetailTouristAreaUser([FromBody] TourismProductDetailRequest req)
        {
            var user = await getUser();

            if (req == null) return BadRequest("Không nhận được dữ liệu");

            if (req.TourismProduct == null)
            {
                Console.WriteLine("Ở đây NULLLLLLLLLLLLLLLLLLLLLLLLLLL");
                throw new BadRequestException("Đối tượng TourismProduct bị null. Hãy check lại chữ hoa/chữ thường trong JSON!");
            }

            _touristAreaService.update_click_tourist_area(req.id, user);

            var data = await _touristAreaService.GetDetailTouristAreasAsync(req.id, req.type, user, req.TourismProduct.page, req.TourismProduct.pageSize);
            var touristArea = data.tourist_Area_Detail;
            var images = await _context.Imgs.Where(img => img.EntityType == "tourist_area" && touristArea.Id == img.EntityId).ToListAsync();

            if (req.type == "TouristPlace")
            {
                var tourist_Place = data.TouristPlaces.Items;
                var touristPlaceIds = tourist_Place.Select(t => t.Id).ToList();
                var imagesTouristPlace = await _context.Imgs.Where(img => img.EntityType == "tourist_place" && touristPlaceIds.Contains(img.EntityId)).ToListAsync();
                var totalCountTouristPlace = tourist_Place.Count();
                var data_result_Tourist_Place = new
                {
                    items = tourist_Place.Select(a => new
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
                        type = "tourist_place",
                        images = imagesTouristPlace.Where(img => img.EntityId == a.Id).ToList(),
                        coverImageUrl = imagesTouristPlace.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                    }),
                    totalCount = totalCountTouristPlace,
                    totalPages = (int)Math.Ceiling(totalCountTouristPlace / (double)req.TourismProduct.pageSize),
                    currentPage = req.TourismProduct.page
                };

                return Ok(new
                {
                    success = true,
                    message = "Thành công",
                    data = new
                    {
                        tourist_Area_Detail = new
                        {
                            id = touristArea.Id,
                            name = touristArea.Name,
                            title = touristArea.Title,
                            address = touristArea.Address,
                            description = touristArea.Description,
                            rating_average = touristArea.RatingAverage,
                            click_count = touristArea.ClickCount,
                            favorite_count = touristArea.FavoriteCount,
                            trending_Score = Math.Round((touristArea.RatingAverage * 10m) + (touristArea.FavoriteCount * 2m) + (touristArea.ClickCount * 0.1m), 2),
                            latitude = touristArea.Latitude,
                            longitude = touristArea.Longitude,
                            type = "tourist_area",
                            images = images.ToList(),
                            coverImageUrl = images.FirstOrDefault(img => img.EntityId == touristArea.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                        },
                        pagedResult = data_result_Tourist_Place
                    }
                });
            }

            var tour = data.Tours.Items;
            var tourIds = tour.Select(t => t.Id).ToList();
            var imagesTour = await _context.Imgs.Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId)).ToListAsync();
            var totalCount = tour.Count();
            var data_result_Tour = new
            {
                items = tour.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    title = a.Title,
                    description = a.Description,
                    durationDays = a.DurationDays,
                    numberOfPeople = a.NumberOfPeople,
                    rating_average = a.RatingAverage,
                    click_count = a.ClickCount,
                    favorite_count = a.FavoriteCount,
                    trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m), 2),
                    type = "tour",
                    images = imagesTour.Where(img => img.EntityId == a.Id).ToList(),
                    coverImageUrl = imagesTour.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                }),
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)req.TourismProduct.pageSize),
                currentPage = req.TourismProduct.page
            };

            return Ok(new
            {
                success = true,
                message = "Thành công",
                data = new
                {
                    tourist_Area_Detail = new
                    {
                        id = touristArea.Id,
                        name = touristArea.Name,
                        title = touristArea.Title,
                        address = touristArea.Address,
                        description = touristArea.Description,
                        rating_average = touristArea.RatingAverage,
                        click_count = touristArea.ClickCount,
                        favorite_count = touristArea.FavoriteCount,
                        trending_Score = Math.Round((touristArea.RatingAverage * 10m) + (touristArea.FavoriteCount * 2m) + (touristArea.ClickCount * 0.1m), 2),
                        latitude = touristArea.Latitude,
                        longitude = touristArea.Longitude,
                        type = "tourist_area",
                        images = images.ToList(),
                        coverImageUrl = images.FirstOrDefault(img => img.EntityId == touristArea.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                    },
                    pagedResult = data_result_Tour
                }
            });
        }

        private async Task<User> getUser()
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

            return user;
        }
    }
}
