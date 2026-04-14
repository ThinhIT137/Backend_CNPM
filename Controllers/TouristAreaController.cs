using backend.DTO;
using backend.Exceptions;
using backend.Hubs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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
        private readonly IHubContext<NotificationHub> _hubContext;
        public TouristAreaController(IJwtService jwtService, CnpmContext cnpmContext, ITouristAreaService touristAreaService, IHubContext<NotificationHub> hubContext)
        {
            _context = cnpmContext;
            _jwtService = jwtService;
            _touristAreaService = touristAreaService;
            _hubContext = hubContext;
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

            if (req == null) throw new BadRequestException("Không nhận được dữ liệu");

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


        [Authorize]
        [HttpPost("DetailTouristAreaUser")]
        public async Task<IActionResult> DetailTouristAreaUser([FromBody] TourismProductDetailRequest req)
        {
            if (req == null) throw new BadRequestException("Không nhận được dữ liệu");
            if (req.TourismProduct == null)
            {
                throw new BadRequestException("Đối tượng TourismProduct bị null. Hãy check lại chữ hoa/chữ thường trong JSON!");
            }

            User currentUser = await getUser();

            await _touristAreaService.update_click_tourist_area(req.id, currentUser);

            var data = await _touristAreaService.GetDetailTouristAreasAsync(req.id, req.type, currentUser, req.TourismProduct.page, req.TourismProduct.pageSize);
            var touristArea = data.tourist_Area_Detail;
            var images = await _context.Imgs.Where(img => img.EntityType == "tourist_area" && touristArea.Id == img.EntityId).ToListAsync();

            bool checkIsFavorite = false;
            if (currentUser != null)
            {
                checkIsFavorite = await _context.Favorites.AnyAsync(i => i.UserId == currentUser.Id && i.EntityId == touristArea.Id && i.EntityType == "tourist_area");
            }

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
                            isFavorite = checkIsFavorite,
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
                        isFavorite = checkIsFavorite,
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTouristArea([FromBody] TouristAreaRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid ownerId = string.IsNullOrEmpty(userIdStr) ? Guid.Empty : Guid.Parse(userIdStr);

            var newEntity = new Tourist_Area
            {
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Description = request.Description,
                Title = request.Title,

                CreatedAt = DateTime.Now,
                ClickCount = 0,
                RatingAverage = 0,
                Created_By_UserId = ownerId,

                // GẮN CỨNG STATUS LÀ ACTIVE ĐỂ NÓ HIỆN LÊN ĐƯỢC BẢNG QUẢN LÝ NÈ SẾP
                Status = "Pending"
            };

            int newId = await _touristAreaService.addTouristArea(newEntity);
            return Ok(new { success = true, message = "Thêm khu du lịch thành công", data = new { id = newId } });
        }

        // ==========================================
        // 2. CẬP NHẬT KHU DU LỊCH
        // PUT: /api/TouristArea/{id}
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTouristArea(int id, [FromBody] TouristAreaRequest request)
        {
            if (request == null || id <= 0)
                return BadRequest(new { success = false, message = "Dữ liệu hoặc ID không hợp lệ" });

            // Vẫn gọi Service bình thường
            await _touristAreaService.UpdateTouristArea(id, request);

            return Ok(new { success = true, message = "Cập nhật khu du lịch thành công" });

        }

        // ==========================================
        // 3. XÓA KHU DU LỊCH
        // DELETE: /api/TouristArea/{id}
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")] // Tương tự PUT, chỉ cần {id}
        public async Task<IActionResult> DeleteTouristArea(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "ID không hợp lệ" });

            await _touristAreaService.RemoveTouristArea(id);

            return Ok(new { success = true, message = "Xóa khu du lịch thành công" });
        }

        // ==========================================
        // 4. LẤY DANH SÁCH KHU DU LỊCH CỦA TÔI
        // GET: /api/TouristArea/my-areas
        // ==========================================
        [Authorize(Roles = "Admin, Owner, Hotel, Tour, User")]
        [HttpGet("my-areas")]
        public async Task<IActionResult> GetMyTouristAreas([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] string? status = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var data = await _touristAreaService.GetMyTouristAreasAsync(Guid.Parse(userIdStr), page, pageSize, keyword, status);
            return Ok(new { success = true, data = data });
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

        [AllowAnonymous]
        [HttpGet("all-dropdown")]
        public async Task<IActionResult> GetAllForDropdown()
        {
            var data = await _touristAreaService.GetAllForDropdownAsync();
            return Ok(new { success = true, data = data });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{id:int}")]
        public async Task<IActionResult> ApproveTouristArea(int id, [FromBody] ApprovalRequest req)
        {
            // 1. Tìm trong DB
            var area = await _context.TouristAreas.FindAsync(id);
            if (area == null) return NotFound(new { success = false, message = "Không tìm thấy khu du lịch" });

            // 2. Đổi status
            area.Status = req.Status; // "Active" hoặc "Rejected"

            // ===============================================
            // 3. TẠO THÔNG BÁO CHO NGƯỜI TẠO KHU DU LỊCH
            // ===============================================
            string notifTitle = "";
            string notifContent = "";

            if (req.Status == "Active" || req.Status == "Approved")
            {
                notifTitle = "✅ Khu du lịch đã được duyệt";
                notifContent = $"Tuyệt vời! Khu du lịch '{area.Name}' của bạn đã được Admin phê duyệt và hiển thị trên hệ thống.";
            }
            else if (req.Status == "Rejected")
            {
                notifTitle = "❌ Khu du lịch bị từ chối";
                notifContent = $"Rất tiếc, khu du lịch '{area.Name}' của bạn chưa đạt yêu cầu và đã bị từ chối. Vui lòng kiểm tra lại thông tin.";
            }

            // Tạo object thông báo
            var notif = new Notification
            {
                UserId = area.Created_By_UserId, // Lấy ID của người đã đăng bài này
                Title = notifTitle,
                Content = notifContent,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notif);

            // 4. Lưu tất cả thay đổi (cả status của Area và Notification mới) vào DB
            await _context.SaveChangesAsync();

            // ===============================================
            // 5. BẮN THÔNG BÁO REAL-TIME QUA SIGNALR
            // ===============================================
            if (_hubContext != null && area.Created_By_UserId != Guid.Empty)
            {
                await _hubContext.Clients.User(area.Created_By_UserId.ToString()).SendAsync("ReceiveNotification", new
                {
                    id = notif.Id,
                    title = notif.Title,
                    content = notif.Content,
                    createdAt = notif.CreatedAt,
                    isRead = false
                });
            }

            return Ok(new { success = true, message = $"Đã duyệt khu du lịch thành {req.Status}" });
        }

        // ==========================================
        // ADMIN: LẤY DANH SÁCH KHU DU LỊCH ĐANG CHỜ DUYỆT
        // GET: /api/TouristArea/admin/pending
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending")]
        public async Task<IActionResult> GetAllPendingTouristAreas([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.TouristAreas.Where(a => a.Status == "Pending");

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var areaIds = items.Select(a => a.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "tourist_area" && areaIds.Contains(img.EntityId) && img.IsCover)
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

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllTouristAreasForAdmin([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null, [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.TouristAreas.AsQueryable();

                // Lọc theo từ khóa (Tên hoặc Địa chỉ)
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(a => a.Name.Contains(keyword) || a.Address.Contains(keyword));
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status == status);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var areaIds = items.Select(a => a.Id).ToList();
                var images = await _context.Imgs
                    .Where(img => img.EntityType == "tourist_area" && areaIds.Contains(img.EntityId) && img.IsCover)
                    .ToListAsync();

                var dataResult = items.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,
                    title = a.Title,
                    address = a.Address,
                    description = a.Description,
                    rating_average = a.RatingAverage,
                    status = a.Status,
                    createdAt = a.CreatedAt,
                    createdByUserId = a.Created_By_UserId,
                    latitude = a.Latitude,
                    longitude = a.Longitude,
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
