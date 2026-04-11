using backend.Data;
using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace backend.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly CnpmContext _context;
        public InteractionService(CnpmContext context) { _context = context; }

        // ==========================================
        // 1. RATING & COMMENT
        // ==========================================
        public async Task AddReviewAsync(Guid userId, ReviewRequest req)
        {
            // 1. Lưu Review mới
            var review = new Review
            {
                UserId = userId,
                EntityId = req.EntityId,
                EntityType = req.EntityType.ToLower(),
                Score = req.Star,
                Comment = req.Content,
                CreatedAt = DateTime.Now
            };
            _context.Reviews.Add(review);

            // 2. Cập nhật Atomic Rating cho bảng đích
            if (req.EntityType.Equals("hotel", StringComparison.OrdinalIgnoreCase))
            {
                var hotel = await _context.Hotels.FindAsync(req.EntityId) ?? throw new NotFoundException("Hotel k có");
                hotel.RatingTotal += req.Star;
                hotel.RatingCount += 1;
                hotel.RatingAverage = (decimal)hotel.RatingTotal / hotel.RatingCount;
            }
            else if (req.EntityType.Equals("tour", StringComparison.OrdinalIgnoreCase))
            {
                var tour = await _context.Tours.FindAsync(req.EntityId) ?? throw new NotFoundException("Tour k có");
                tour.RatingTotal += req.Star;
                tour.RatingCount += 1;
                tour.RatingAverage = (decimal)tour.RatingTotal / tour.RatingCount;
            }

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // 2. TOGGLE FAVORITE (Lưu/Bỏ lưu)
        // ==========================================
        public async Task<bool> ToggleFavoriteAsync(Guid userId, FavoriteRequest req)
        {
            var fav = await _context.Favorites.AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(f =>
                f.UserId == userId && f.EntityId == req.EntityId && f.EntityType == req.EntityType.ToLower());

            bool isAdded;
            if (fav != null)
            {
                _context.Favorites.Remove(fav);
                isAdded = false;
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    EntityId = req.EntityId,
                    EntityType = req.EntityType.ToLower(),
                    CreatedAt = DateTime.Now
                });
                isAdded = true;
            }

            // Cập nhật count ở bảng chính để filter cho nhanh
            await UpdateFavoriteCount(req.EntityId, req.EntityType, isAdded ? 1 : -1);

            await _context.SaveChangesAsync();
            return isAdded;
        }

        private async Task UpdateFavoriteCount(int id, string type, int change)
        {
            if (type == "hotel")
            {
                var h = await _context.Hotels.FindAsync(id);
                if (h != null) h.FavoriteCount += change;
            }
            else if (type == "tour")
            {
                var t = await _context.Tours.FindAsync(id);
                if (t != null) t.FavoriteCount += change;
            }
            // Tương tự cho tourist_area...
        }

        // ==========================================
        // 3. MAP MARKERS
        // ==========================================
        public async Task<int> CreateMarkerAsync(Guid userId, MarkerRequest req)
        {
            // Rào lỗi: Check xem Địa điểm du lịch này có tồn tại không
            var isPlaceExist = await _context.TouristPlaces.AnyAsync(p => p.Id == req.Tourist_Place_Id);
            if (!isPlaceExist)
            {
                throw new NotFoundException("Không tìm thấy địa điểm du lịch này, không thể ghim vị trí!");
            }

            var marker = new Marker
            {
                CreatedByUserId = userId,
                Title = req.Title,
                Description = req.Description,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                IsPublic = req.IsPublic,
                TouristPlaceId = req.Tourist_Place_Id, // BỔ SUNG DÒNG NÀY ĐỂ MAP DỮ LIỆU NÈ SẾP
                CreatedAt = DateTime.Now
            };

            _context.Markers.Add(marker);
            await _context.SaveChangesAsync();

            return marker.Id;
        }

        public async Task UpdateMarkerAsync(Guid userId, int markerId, MarkerRequest req)
        {
            var marker = await _context.Markers.FindAsync(markerId);
            if (marker == null)
            {
                throw new NotFoundException("Không tìm thấy vị trí đã ghim này!");
            }

            if (marker.CreatedByUserId != userId)
            {
                throw new ForbiddenException("Sếp không có quyền sửa vị trí của người khác!");
            }

            var isPlaceExist = await _context.TouristPlaces.AnyAsync(p => p.Id == req.Tourist_Place_Id);
            if (!isPlaceExist)
            {
                throw new NotFoundException("Địa điểm du lịch không tồn tại!");
            }

            // Cập nhật thông tin mới
            marker.Title = req.Title;
            marker.Description = req.Description;
            marker.Latitude = req.Latitude;
            marker.Longitude = req.Longitude;
            marker.IsPublic = req.IsPublic;
            marker.TouristPlaceId = req.Tourist_Place_Id;

            await _context.SaveChangesAsync();
        }

        // 🔴 THÊM: XÓA VỊ TRÍ ĐÃ GHIM (MARKER)
        public async Task DeleteMarkerAsync(Guid userId, int markerId)
        {
            var marker = await _context.Markers.FindAsync(markerId);
            if (marker == null)
            {
                throw new NotFoundException("Không tìm thấy vị trí đã ghim này!");
            }

            if (marker.CreatedByUserId != userId)
            {
                throw new ForbiddenException("Sếp không có quyền xóa vị trí của người khác!");
            }

            _context.Markers.Remove(marker);
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // LẤY DANH SÁCH MARKER CỦA TÔI (CÓ KÈM ẢNH ĐỂ ĐI CHƠI)
        // ==========================================
        public async Task<object> GetMyMarkersAsync(Guid userId, string? keyword = null)
        {
            var query = _context.Markers.Where(m => m.CreatedByUserId == userId).AsNoTracking();

            var allMarkers = await query.ToListAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower().Trim();

                var scoredMarkers = allMarkers.Select(m =>
                {
                    string unSignTitle = StringHelper.ConvertToUnSign(m.Title ?? "").ToLower();
                    string unSignDesc = StringHelper.ConvertToUnSign(m.Description ?? "").ToLower();

                    double maxSimilarity = new[] {
                StringHelper.CalculateSimilarity(unSignKeyword, unSignTitle) * 3.0,
                StringHelper.CalculateSimilarity(unSignKeyword, unSignDesc) * 1.5
            }.Max();

                    if (unSignTitle.Contains(unSignKeyword)) maxSimilarity += 5.0;

                    return new { Item = m, Score = maxSimilarity };
                })
                .Where(x => x.Score >= 0.8)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.CreatedAt)
                .ToList();

                allMarkers = scoredMarkers.Select(x => x.Item).ToList();
            }
            else
            {
                allMarkers = allMarkers.OrderByDescending(m => m.CreatedAt).ToList();
            }

            var markerIds = allMarkers.Select(m => m.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "marker" && markerIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = allMarkers.Select(m => new
            {
                id = m.Id,
                title = m.Title,
                description = m.Description,
                latitude = m.Latitude,
                longitude = m.Longitude,
                isPublic = m.IsPublic,
                tourist_Place_Id = m.TouristPlaceId,
                images = images.Where(img => img.EntityId == m.Id).Select(img => new { id = img.Id, url = img.url, isCover = img.IsCover }).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == m.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return dataResult;
        }

        // ==========================================
        // LẤY DANH SÁCH CÁC ĐÁNH GIÁ MÀ DỊCH VỤ CỦA TÔI NHẬN ĐƯỢC
        // ==========================================
        public async Task<object> GetReviewsReceivedAsync(Guid ownerId, int page = 1, int pageSize = 10)
        {
            // 1. Gom tất cả ID của các dịch vụ do ownerId này tạo ra
            var myHotelIds = await _context.Hotels.Where(h => h.Created_By_UserId == ownerId).Select(h => h.Id).ToListAsync();
            var myTourIds = await _context.Tours.Where(t => t.Created_By_UserId == ownerId).Select(t => t.Id).ToListAsync();
            var myAreaIds = await _context.TouristAreas.Where(a => a.Created_By_UserId == ownerId).Select(a => a.Id).ToListAsync();
            var myPlaceIds = await _context.TouristPlaces.Where(p => p.Created_By_UserId == ownerId).Select(p => p.Id).ToListAsync();

            // 2. Tìm tất cả Review nhắm vào đống ID vừa tìm được
            var query = _context.Reviews.Where(r =>
                (r.EntityType == "hotel" && myHotelIds.Contains(r.EntityId ?? -1)) ||
                (r.EntityType == "tour" && myTourIds.Contains(r.EntityId ?? -1)) ||
                (r.EntityType == "tourist_area" && myAreaIds.Contains(r.EntityId ?? -1)) ||
                (r.EntityType == "tourist_place" && myPlaceIds.Contains(r.EntityId ?? -1))
            ).AsNoTracking();

            var totalCount = await query.CountAsync();

            // 3. Phân trang và lấy dữ liệu
            var data = await query.OrderByDescending(r => r.CreatedAt)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(r => new
                                  {
                                      id = r.Id,
                                      entityType = r.EntityType, // Khách sạn hay Tour
                                      entityId = r.EntityId,
                                      score = r.Score,
                                      comment = r.Comment,
                                      createdAt = r.CreatedAt,
                                      reviewerId = r.UserId // ID người đánh giá
                                  })
                                  .ToListAsync();

            // 4. Trả về format chuẩn phân trang
            return new
            {
                items = data,
                totalCount = totalCount,
                totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page
            };
        }
    }
}
