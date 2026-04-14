using backend.Data;
using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Services
{
    public class TouristPlaceService : ITouristPlaceService
    {
        public CnpmContext _context { get; set; }
        public TouristPlaceService(CnpmContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Tourist_Place>> GetPagedResult(int page = 1, int pageSize = 10)
        {
            var query = _context.TouristPlaces;

            var TotalCount = await query.CountAsync();
            var data = await query.OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .AsNoTracking().AsSplitQuery()
                            .ToListAsync();

            if (data == null)
            {
                throw new BadRequestException("Không có địa điểm du lịch");
            }

            Console.WriteLine("page :" + (int)Math.Ceiling(TotalCount / (double)pageSize));

            return new PagedResult<Tourist_Place>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task<PagedResult<Tourist_Place>> GetPagedResult(int Tourist_Area_Id, int page = 1, int pageSize = 10)
        {
            var query = _context.TouristPlaces.Where(tp => tp.Tourist_Area_Id == Tourist_Area_Id);

            var TotalCount = await query.CountAsync();
            var data = await query.OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .AsNoTracking().AsSplitQuery()
                            .ToListAsync();

            if (data == null)
            {
                throw new BadRequestException("Không có địa điểm du lịch");
            }

            return new PagedResult<Tourist_Place>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task<PagedResult<Tourist_Place>> GetPagedResult(User u, int page = 1, int pageSize = 10)
        {
            var history = GetHistoryUser(u);

            var historyList = history.TouristPlace ?? new List<int>();

            var query = _context.TouristPlaces;
            var TotalCount = await query.CountAsync();

            var data = await query
                        .OrderByDescending(t =>
                            (t.RatingAverage * 10m) +
                            (t.FavoriteCount * 2m) +
                            (t.ClickCount * 0.1m) +
                            (historyList.Contains(t.Id) ? 50m : 0m)
                        )
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .AsNoTracking().AsSplitQuery()
                        .ToListAsync();

            if (data == null)
            {
                throw new BadRequestException("Không có địa điểm du lịch");
            }

            return new PagedResult<Tourist_Place>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task<object> GetMyTouristPlacesAsync(Guid userId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null)
        {
            var query = _context.TouristPlaces.Where(t => t.Created_By_UserId == userId).AsNoTracking().AsSplitQuery();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            var allPlaces = await query.ToListAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower().Trim();

                var scoredPlaces = allPlaces.Select(p =>
                {
                    string unSignName = StringHelper.ConvertToUnSign(p.Name ?? "").ToLower();
                    string unSignTitle = StringHelper.ConvertToUnSign(p.Title ?? "").ToLower();
                    string unSignAddress = StringHelper.ConvertToUnSign(p.Address ?? "").ToLower();

                    double maxSimilarity = new[] {
                StringHelper.CalculateSimilarity(unSignKeyword, unSignName) * 3.0,
                StringHelper.CalculateSimilarity(unSignKeyword, unSignTitle) * 2.0,
                StringHelper.CalculateSimilarity(unSignKeyword, unSignAddress) * 1.5
            }.Max();

                    if (unSignName.Contains(unSignKeyword) || unSignAddress.Contains(unSignKeyword)) maxSimilarity += 5.0;

                    return new { Item = p, Score = maxSimilarity };
                })
                .Where(x => x.Score >= 0.8)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.CreatedAt)
                .ToList();

                allPlaces = scoredPlaces.Select(x => x.Item).ToList();
            }
            else
            {
                allPlaces = allPlaces.OrderByDescending(p => p.CreatedAt).ToList();
            }

            var totalCount = allPlaces.Count;
            var pagedPlaces = allPlaces.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var placeIds = pagedPlaces.Select(p => p.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "tourist_place" && placeIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = pagedPlaces.Select(a => new
            {
                id = a.Id,
                name = a.Name,
                title = a.Title,
                address = a.Address,
                description = a.Description,
                rating_average = a.RatingAverage,
                click_count = a.ClickCount,
                favorite_count = a.FavoriteCount,
                latitude = a.Latitude,
                longitude = a.Longitude,
                tourist_Area_Id = a.Tourist_Area_Id,
                status = a.Status,
                type = "tourist_place",
                images = images.Where(img => img.EntityId == a.Id).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return new { items = dataResult, totalCount = totalCount, totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize), currentPage = page };
        }

        public async Task<int> CreateTouristPlaceAsync(TouristPlaceRequest req, Guid ownerId)
        {
            var place = new Tourist_Place
            {
                Name = req.Name,
                Title = req.Title,
                Address = req.Address,
                Description = req.Description,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                Tourist_Area_Id = req.Tourist_Area_Id,
                Created_By_UserId = ownerId,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            _context.TouristPlaces.Add(place);
            await _context.SaveChangesAsync();

            return place.Id; // QUAN TRỌNG: TRẢ VỀ CÁI ID NÀY
        }

        public async Task UpdateTouristPlaceAsync(int id, TouristPlaceRequest req, Guid ownerId)
        {
            var place = await _context.TouristPlaces.FindAsync(id);
            if (place == null) throw new NotFoundException("Không tìm thấy địa điểm");

            // Check quyền chính chủ (Admin hoặc đúng chủ)
            if (place.Created_By_UserId != ownerId) throw new ForbiddenException("Không có quyền sửa!");

            place.Name = req.Name;
            place.Title = req.Title;
            place.Address = req.Address;
            place.Description = req.Description;
            place.Latitude = req.Latitude;
            place.Longitude = req.Longitude;
            place.Tourist_Area_Id = req.Tourist_Area_Id;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTouristPlaceAsync(int id, Guid ownerId)
        {
            var place = await _context.TouristPlaces.FindAsync(id);
            if (place == null) throw new NotFoundException("Không tìm thấy địa điểm");

            if (place.Created_By_UserId != ownerId) throw new ForbiddenException("Không có quyền xóa!");

            _context.TouristPlaces.Remove(place);
            await _context.SaveChangesAsync();
        }

        private UserSearchHistory GetHistoryUser(User user)
        {
            if (string.IsNullOrEmpty(user.User_Search_History))
            {
                return new UserSearchHistory();
            }
            return JsonConvert.DeserializeObject<UserSearchHistory>(user.User_Search_History) ?? new UserSearchHistory();
        }

        private void UpdateHistoryQueue(List<int> historyList, int newId, int maxItems = 5)
        {
            if (historyList == null) return;

            historyList.Remove(newId);

            historyList.Insert(0, newId);

            if (historyList.Count > maxItems)
            {
                historyList.RemoveAt(historyList.Count - 1);
            }
        }

        public async Task<object> GetAllForDropdownAsync()
        {
            var data = await _context.TouristPlaces.AsNoTracking()
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    title = p.Title,
                    tourist_Area_Id = p.Tourist_Area_Id,
                    latitude = p.Latitude,
                    longitude = p.Longitude
                })
                .ToListAsync();
            return data;
        }
    }
}
