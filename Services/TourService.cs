using backend.Data;
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
            var query = _context.Tours.Where(h => h.Status == "Active").AsQueryable();
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
                            .AsNoTracking().AsSplitQuery()
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
            var query = _context.Tours.Where(tp => tp.Tourist_Area_Id == Tourist_Area_Id && tp.Status == "Active").AsNoTracking().AsSplitQuery().AsQueryable();
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
                            .AsNoTracking().AsSplitQuery()
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
            var query = _context.Tours.Where(t => t.Tour_Itinerarys.Any(ti => ti.Tourist_Place_Id == touristPlaceId) && t.Status == "Active").AsQueryable();
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
                                  .AsNoTracking().AsSplitQuery()
                                  .ToListAsync();

            //if (data == null || data.Count == 0) throw new BadRequestException("Không có chuyến du lịch nào đi qua địa điểm này");

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
                .Include(t => t.Departures)
                .AsNoTracking().AsSplitQuery()
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null)
            {
                throw new BadRequestException("Không tìm thấy thông tin chuyến du lịch này.");
            }

            return tour;
        }

        // --- CRUD TOUR ---
        public async Task<int> CreateTourAsync(TourRequest req, Guid ownerId)
        {
            var tour = new Tour
            {
                Name = req.Name,
                Description = req.Description,
                Title = req.Title,
                DurationDays = req.DurationDays,
                NumberOfPeople = req.NumberOfPeople,
                DepartureLocationName = req.DepartureLocationName,
                DepartureLatitude = req.DepartureLatitude,
                DepartureLongitude = req.DepartureLongitude,
                Vehicle = req.Vehicle,
                TourType = req.TourType,
                Price = req.Price,
                Tourist_Area_Id = req.Tourist_Area_Id,
                Created_By_UserId = ownerId,
                Status = "Pending", // Chờ Admin duyệt
                CreatedAt = DateTime.Now
            };
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return tour.Id; // QUAN TRỌNG NHẤT LÀ DÒNG NÀY NÈ SẾP!
        }

        public async Task UpdateTourAsync(int id, TourRequest req, Guid ownerId)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) throw new NotFoundException("Không tìm thấy Tour");
            if (tour.Created_By_UserId != ownerId) throw new ForbiddenException("Cấm đụng vào Tour của người khác!");

            tour.Name = req.Name; tour.Description = req.Description; tour.Title = req.Title;
            tour.DurationDays = req.DurationDays; tour.NumberOfPeople = req.NumberOfPeople;
            tour.DepartureLocationName = req.DepartureLocationName;
            tour.DepartureLatitude = req.DepartureLatitude; tour.DepartureLongitude = req.DepartureLongitude;
            tour.Vehicle = req.Vehicle; tour.TourType = req.TourType; tour.Price = req.Price;
            tour.Tourist_Area_Id = req.Tourist_Area_Id;
            tour.Status = "Pending";

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTourAsync(int id, Guid ownerId)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) throw new NotFoundException("Không tìm thấy Tour");
            if (tour.Created_By_UserId != ownerId) throw new ForbiddenException("Cấm đụng vào Tour của người khác!");

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
        }

        // --- CRUD LỊCH TRÌNH (ITINERARY) ---
        public async Task AddItineraryAsync(int tourId, TourItineraryRequest req, Guid ownerId)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null) throw new NotFoundException("Không tìm thấy Tour");
            if (tour.Created_By_UserId != ownerId) throw new ForbiddenException("Không thể thêm lịch trình vào Tour của người khác!");

            var itinerary = new Tour_Itinerary
            {
                TourId = tourId,
                Title = req.Title,
                Description = req.Description,
                Tourist_Place_Id = req.Tourist_Place_Id,
                DayNumber = req.DayNumber
            };
            _context.TourItineraries.Add(itinerary);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItineraryAsync(int itineraryId, TourItineraryRequest req, Guid ownerId)
        {
            var itinerary = await _context.TourItineraries.Include(ti => ti.Tour).AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(ti => ti.Id == itineraryId);
            if (itinerary == null) throw new NotFoundException("Không tìm thấy lịch trình");

            // Check qua bảng Tour để xem ai là chủ
            if (itinerary.Tour!.Created_By_UserId != ownerId) throw new ForbiddenException("Bạn không có quyền sửa lịch trình này!");

            itinerary.Title = req.Title;
            itinerary.Description = req.Description;
            itinerary.Tourist_Place_Id = req.Tourist_Place_Id;
            itinerary.DayNumber = req.DayNumber;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteItineraryAsync(int itineraryId, Guid ownerId)
        {
            var itinerary = await _context.TourItineraries.Include(ti => ti.Tour).AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(ti => ti.Id == itineraryId);
            if (itinerary == null) throw new NotFoundException("Không tìm thấy lịch trình");

            if (itinerary.Tour!.Created_By_UserId != ownerId) throw new ForbiddenException("Bạn không có quyền xóa lịch trình này!");

            _context.TourItineraries.Remove(itinerary);
            await _context.SaveChangesAsync();
        }

        public async Task ApproveTourAsync(int id, string status)
        {
            var validStatuses = new[] { "Approved", "Rejected", "Pending" };
            if (!validStatuses.Contains(status))
                throw new BadRequestException("Trạng thái không hợp lệ. Chỉ nhận: Approved, Rejected, Pending");

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
                throw new NotFoundException("Không tìm thấy Tour này");

            tour.Status = status;
            await _context.SaveChangesAsync();
        }

        // Đè cái hàm này vào TourService.cs
        public async Task<object> GetMyToursAsync(Guid userId, int page = 1, int pageSize = 10, string? keyword = null, string? status = null)
        {
            var query = _context.Tours
                .Include(t => t.Tour_Itinerarys)
                .Include(t => t.Departures)
                .Where(t => t.Created_By_UserId == userId)
                .AsNoTracking()
                .AsSplitQuery();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            var allTours = await query.ToListAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower().Trim();

                var scoredTours = allTours.Select(t =>
                {
                    string unSignName = StringHelper.ConvertToUnSign(t.Name ?? "").ToLower();
                    string unSignTitle = StringHelper.ConvertToUnSign(t.Title ?? "").ToLower();
                    string unSignDeparture = StringHelper.ConvertToUnSign(t.DepartureLocationName ?? "").ToLower();

                    double maxSimilarity = new[] {
                StringHelper.CalculateSimilarity(unSignKeyword, unSignName) * 3.0,
                StringHelper.CalculateSimilarity(unSignKeyword, unSignTitle) * 2.0,
                StringHelper.CalculateSimilarity(unSignKeyword, unSignDeparture) * 1.5
            }.Max();

                    if (unSignName.Contains(unSignKeyword)) maxSimilarity += 5.0;

                    return new { Item = t, Score = maxSimilarity };
                })
                .Where(x => x.Score >= 0.8)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.CreatedAt)
                .ToList();

                allTours = scoredTours.Select(x => x.Item).ToList();
            }
            else
            {
                allTours = allTours.OrderByDescending(t => t.CreatedAt).ToList();
            }

            var totalCount = allTours.Count;
            var pagedTours = allTours.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var tourIds = pagedTours.Select(t => t.Id).ToList();
            var images = await _context.Imgs
                .Where(img => img.EntityType == "tour" && tourIds.Contains(img.EntityId))
                .ToListAsync();

            var dataResult = pagedTours.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                title = t.Title,
                description = t.Description,
                durationDays = t.DurationDays,
                numberOfPeople = t.NumberOfPeople,
                price = t.Price,
                vehicle = t.Vehicle,
                tourType = t.TourType,
                status = t.Status,
                departureLocationName = t.DepartureLocationName,
                departureLatitude = t.DepartureLatitude,
                departureLongitude = t.DepartureLongitude,
                tourist_Area_Id = t.Tourist_Area_Id,
                tour_Itinerarys = t.Tour_Itinerarys,
                departures = t.Departures,
                images = images.Where(img => img.EntityId == t.Id).Select(img => new { id = img.Id, url = img.url, isCover = img.IsCover }).ToList()
            });

            return new { items = dataResult, totalCount = totalCount, totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize), currentPage = page };
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