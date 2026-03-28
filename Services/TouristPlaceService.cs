using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
