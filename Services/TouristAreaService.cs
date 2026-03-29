using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Collections;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

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
            var history = String.IsNullOrEmpty(u.User_Search_History) ? new UserSearchHistory() : JsonConvert.DeserializeObject<UserSearchHistory>(u.User_Search_History);
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
            var Page_Result_Tour = await _tourService.GetPagedResult(id, page, pageSize);

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
    }
}
