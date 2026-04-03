using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Services
{
    public class TourService : ITourService
    {
        public CnpmContext _context { get; set; }
        public TourService(CnpmContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Tour>> GetPagedResult(User? user, int page = 1, int pageSize = 10)
        {
            var query = _context.Tours.AsQueryable();
            var TotalCount = await query.CountAsync();

            var historyList = new List<int>();
            if (user != null)
            {
                var history = GetHistoryUser(user);
                historyList = history.Tour ?? new List<int>();
            }

            var data = await query.OrderByDescending(t =>
                            ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m) + (historyList.Contains(t.Id) ? 50m : 0m)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            if (data == null)
            {
                throw new BadRequestException("Không có chuyến du lịch");
            }

            return new PagedResult<Tour>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task<PagedResult<Tour>> GetPagedResult(int Tourist_Area_Id, User? user, int page = 1, int pageSize = 10)
        {
            var query = _context.Tours.Where(tp => tp.Tourist_Area_Id == Tourist_Area_Id);
            var TotalCount = await query.CountAsync();

            var historyList = new List<int>();
            if (user != null)
            {
                var history = GetHistoryUser(user);
                historyList = history.Tour ?? new List<int>();
            }

            var data = await query.OrderByDescending(t =>
                            ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m) + (historyList.Contains(t.Id) ? 50m : 0m)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            if (data == null)
            {
                throw new BadRequestException("Không có địa điểm du lịch");
            }

            return new PagedResult<Tour>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }

        public async Task<PagedResult<Tour>> GetToursByTouristPlaceId(int touristPlaceId, User? user, int page = 1, int pageSize = 10)
        {
            var query = _context.Tours.Where(t => t.Tour_Itinerarys.Any(ti => ti.Tourist_Place_Id == touristPlaceId));
            var TotalCount = await query.CountAsync();

            var historyList = new List<int>();
            if (user != null)
            {
                var history = GetHistoryUser(user);
                historyList = history.Tour ?? new List<int>();
            }

            var data = await query.OrderByDescending(t =>
                                    ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m) + (historyList.Contains(t.Id) ? 50m : 0m)))
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            if (data == null || data.Count == 0) throw new BadRequestException("Không có chuyến du lịch nào đi qua địa điểm này");

            return new PagedResult<Tour>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }
        public async Task<Tour> GetTourDetail(int tourId)
        {
            var tour = await _context.Tours
                .Include(t => t.Tourist_Area)
                .Include(t => t.Tour_Itinerarys)
                .ThenInclude(ti => ti.Tourist_Place)
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null)
            {
                throw new BadRequestException("Không tìm thấy thông tin chuyến du lịch này.");
            }

            return tour;
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