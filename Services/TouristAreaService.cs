using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Services
{
    public class TouristAreaService : ITouristAreaService
    {
        private readonly CnpmContext _context;
        private readonly ITouristPlaceService _touristPlaceService;
        private readonly ITourService _tourService;

        public TouristAreaService(CnpmContext context, ITouristPlaceService touristPlaceService, ITourService tourService)
        {
            _context = context;
            _touristPlaceService = touristPlaceService;
            _tourService = tourService;
        }

        public async Task<PagedResult<Tourist_Area>> GetTrendingTouristAreasAsync(User u, int page = 1, int pageSize = 10)
        {
            var history = GetHistoryUser(u);
            var query = _context.TouristAreas;
            var TotalCount = await query.CountAsync();
            var data = await query
                            .OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m) + (history.TouristArea.Contains(t.Id) ? 50 : 0)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
            if (data == null)
                throw new BadRequestException("Lấy dữ liệu không thành công");

            return new PagedResult<Tourist_Area>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }
        public async Task<PagedResult<Tourist_Area>> GetTrendingTouristAreasAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.TouristAreas;
            var TotalCount = await query.CountAsync();
            var data = await query
                            .OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
            if (data == null)
                throw new BadRequestException("Lấy dữ liệu không thành công");

            return new PagedResult<Tourist_Area>
            {
                Items = data,
                TotalCount = TotalCount,
                TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize),
                CurrentPage = page
            };
        }
        public async Task<TouristAreaDetailResponse> GetDetailTouristAreasAsync(int id, string type, int page = 1, int pageSize = 10)
        {
            // Lấy tour place 
            if (type == "TouristPlace")
            {
                var Page_Result_Tourist_Place = await _touristPlaceService.GetPagedResult(id, page, pageSize);

                if (Page_Result_Tourist_Place == null)
                {
                    throw new BadRequestException("Địa điểm du lịch tại khu du lịch đang trống");
                }

                var dataTourist = await _context.TouristAreas.Include(ti => ti.Tourist_Places.OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)).FirstOrDefaultAsync(t => t.Id == id);
                if (dataTourist == null)
                    throw new BadRequestException("Lấy dữ liệu không thành công");

                return new TouristAreaDetailResponse
                {
                    tourist_Area_Detail = dataTourist,
                    TouristPlaces = Page_Result_Tourist_Place
                };
            }

            // Lấy tour 
            var Page_Result_Tour = await _tourService.GetPagedResult(id, null, page, pageSize);

            if (Page_Result_Tour == null)
            {
                throw new BadRequestException("Chuyến du lịch tại khu du lịch đang trống");
            }

            var dataTourist_Tour = await _context.TouristAreas.Include(ti => ti.Tours.OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m)))
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)).FirstOrDefaultAsync(t => t.Id == id);
            if (dataTourist_Tour == null)
                throw new BadRequestException("Lấy dữ liệu không thành công");

            return new TouristAreaDetailResponse
            {
                tourist_Area_Detail = dataTourist_Tour,
                Tours = Page_Result_Tour
            };
        }

        public async Task<TouristAreaDetailResponse> GetDetailTouristAreasAsync(int id, string type, User user, int page = 1, int pageSize = 10)
        {
            var history = GetHistoryUser(user);

            // Lấy tour place 
            if (type == "TouristPlace")
            {

                var Page_Result_Tourist_Place = await _touristPlaceService.GetPagedResult(id, page, pageSize);

                if (Page_Result_Tourist_Place == null)
                {
                    throw new BadRequestException("Địa điểm du lịch tại khu du lịch đang trống");
                }

                var dataTourist = await _context.TouristAreas.Include(ti => ti.Tourist_Places.OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m) + (history.TouristPlace.Contains(t.Id) ? 50 : 0)))
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)).FirstOrDefaultAsync(t => t.Id == id);
                if (dataTourist == null)
                    throw new BadRequestException("Lấy dữ liệu không thành công");

                return new TouristAreaDetailResponse
                {
                    tourist_Area_Detail = dataTourist,
                    TouristPlaces = Page_Result_Tourist_Place
                };
            }

            // Lấy tour 
            var Page_Result_Tour = await _tourService.GetPagedResult(id, user, page, pageSize);

            if (Page_Result_Tour == null)
            {
                throw new BadRequestException("Chuyến du lịch tại khu du lịch đang trống");
            }

            var dataTourist_Tour = await _context.TouristAreas.Include(ti => ti.Tours.OrderByDescending(t => ((t.RatingAverage * 10m) + (t.FavoriteCount * 2m) + (t.ClickCount * 0.1m) + (history.Tour.Contains(t.Id) ? 50 : 0)))
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)).FirstOrDefaultAsync(t => t.Id == id);
            if (dataTourist_Tour == null)
                throw new BadRequestException("Lấy dữ liệu không thành công");

            return new TouristAreaDetailResponse
            {
                tourist_Area_Detail = dataTourist_Tour,
                Tours = Page_Result_Tour
            };
        }

        public async Task update_click_tourist_area(int Tourist_Area_Id, User user)
        {
            var history = GetHistoryUser(user);
            if (history.TouristArea == null) history.TouristArea = new List<int>();

            UpdateHistoryQueue(history.TouristArea, Tourist_Area_Id);
            user.User_Search_History = JsonConvert.SerializeObject(history);
            var tourist_area = await _context.TouristAreas.FirstOrDefaultAsync(ta => ta.Id == Tourist_Area_Id);
            if (tourist_area != null)
            {
                tourist_area.ClickCount += 1;
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task addTouristArea(Tourist_Area tourist)
        {
            var TouristArea = await _context.TouristAreas.Where(t => t.Name == tourist.Name).FirstOrDefaultAsync();

            if (TouristArea != null)
            {
                throw new BadRequestException("Khu du lịch này đã có");
            }

            _context.TouristAreas.Add(tourist);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTouristArea(int id)
        {
            var TouristArea = await _context.TouristAreas.Where(t => t.Id == id).FirstOrDefaultAsync();

            if (TouristArea == null)
            {
                throw new BadRequestException("Khu du lịch này k có");
            }

            _context.TouristAreas.Remove(TouristArea);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTouristArea(int id, Tourist_Area tourist)
        {
            // 2. Dùng FindAsync tìm theo ID cho chuẩn và nhanh
            var existingArea = await _context.TouristAreas.FindAsync(id);

            if (existingArea == null)
            {
                throw new BadRequestException("Khu du lịch này k có");
            }

            existingArea.Name = tourist.Name;
            existingArea.Title = tourist.Title;
            existingArea.Address = tourist.Address;
            existingArea.Description = tourist.Description;
            existingArea.Latitude = tourist.Latitude;
            existingArea.Longitude = tourist.Longitude;

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
    }
}
