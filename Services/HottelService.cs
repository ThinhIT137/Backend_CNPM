using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Services
{
    public class HottelService : IHottelService
    {
        public CnpmContext _context { get; set; }
        public HottelService(CnpmContext context)
        {
            _context = context;
        }

        public async Task<object> GetTrendingHottelAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Hottels.AsQueryable();

            var totalCount = await query.CountAsync();

            var hotels = await query
                .OrderByDescending(h => (h.RatingAverage * 10m) + (h.FavoriteCount * 2m) + (h.ClickCount * 0.1m))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var hotelIds = hotels.Select(h => h.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = hotels.Select(a => new
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

            return new
            {
                items = dataResult,
                totalCount = totalCount,
                totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page
            };
        }

        public async Task<object> GetTrendingHottelAsync(User u, int page = 1, int pageSize = 10)
        {
            var query = _context.Hottels.AsQueryable();
            var totalCount = await query.CountAsync();

            var history = GetHistoryUser(u);
            var recentHotels = history.Hottel ?? new List<int>();

            var hotels = await query
                .OrderByDescending(h =>
                    (h.RatingAverage * 10m) +
                    (h.FavoriteCount * 2m) +
                    (h.ClickCount * 0.1m) +
                    (recentHotels.Contains(h.Id) ? 50m : 0m)
                )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var hotelIds = hotels.Select(h => h.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "hotel" && hotelIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = hotels.Select(a => new
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
                trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m) + (recentHotels.Contains(a.Id) ? 50m : 0m), 2),
                latitude = a.Latitude,
                longitude = a.Longitude,
                type = "hotel",
                images = images.Where(img => img.EntityId == a.Id).ToList(),
                coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            });

            return new
            {
                items = dataResult,
                totalCount = totalCount,
                totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page
            };
        }

        public async Task<PagedResult<Hottel>> GetHotelsByTouristPlaceId(int touristPlaceId, User? user, int page = 1, int pageSize = 10)
        {
            var query = _context.Hottels.Where(h => h.Tourist_Place_Id == touristPlaceId);
            var TotalCount = await query.CountAsync();

            var historyList = new List<int>();
            if (user != null)
            {
                var history = GetHistoryUser(user);
                historyList = history.Hottel ?? new List<int>();
            }

            var data = await query.OrderByDescending(h => ((h.RatingAverage * 10m) + (h.FavoriteCount * 2m) + (h.ClickCount * 0.1m) + (historyList.Contains(h.Id) ? 50m : 0m)))
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            if (data == null || data.Count == 0)
            {
                throw new BadRequestException("Không có khách sạn nào gần địa điểm này");
            }

            return new PagedResult<Hottel>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task<object> GetHotelDetailAsync(int id, User? user)
        {
            var hotel = await _context.Hottels
                .Include(h => h.Tourist_Place)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hotel == null)
            {
                throw new BadRequestException("Không tìm thấy khách sạn này");
            }

            hotel.ClickCount += 1;

            if (user != null)
            {
                var history = GetHistoryUser(user);
                if (history.Hottel == null) history.Hottel = new List<int>();

                UpdateHistoryQueue(history.Hottel, id);

                user.User_Search_History = JsonConvert.SerializeObject(history);
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            var images = await _context.Imgs
                .Where(img => img.EntityType == "hotel" && img.EntityId == id)
                .ToListAsync();

            return new
            {
                id = hotel.Id,
                name = hotel.Name,
                title = hotel.Title,
                address = hotel.Address,
                description = hotel.Description,
                price = hotel.Price,
                latitude = hotel.Latitude,
                longitude = hotel.Longitude,
                status = hotel.Status,
                number_of_people = hotel.NumberOfPeople,
                rating_average = hotel.RatingAverage,
                rating_count = hotel.RatingCount,
                click_count = hotel.ClickCount,
                favorite_count = hotel.FavoriteCount,
                trending_Score = Math.Round((hotel.RatingAverage * 10m) + (hotel.FavoriteCount * 2m) + (hotel.ClickCount * 0.1m), 2),
                tourist_place = hotel.Tourist_Place != null ? new
                {
                    id = hotel.Tourist_Place.Id,
                    name = hotel.Tourist_Place.Name
                } : null,
                images = images,
                coverImageUrl = images.FirstOrDefault(img => img.IsCover)?.url ?? "/Img/ImgNull.jpg"
            };
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
    }
}