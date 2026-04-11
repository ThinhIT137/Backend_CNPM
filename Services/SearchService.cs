using backend.Data;
using backend.DTO;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class SearchService : ISearchService
    {
        private readonly CnpmContext _context;

        public SearchService(CnpmContext context)
        {
            _context = context;
        }

        public async Task<object> FilterAsync(SearchFilterRequest req)
        {
            if (string.IsNullOrEmpty(req.Type))
                throw new BadRequestException("Vui lòng truyền lên Type (Tour, Hotel, TouristArea, TouristPlace)");

            var keyword = req.Keyword?.Trim() ?? "";

            switch (req.Type.ToLower())
            {
                case "touristarea":
                    {
                        var areaQuery = _context.TouristAreas.AsQueryable();

                        var allAreas = await areaQuery
                            .Select(a => new
                            {
                                a.Id,
                                a.Name,
                                a.Title,
                                a.Address,
                                a.Description,
                                a.RatingAverage,
                                a.ClickCount,
                                a.FavoriteCount,
                                a.Latitude,
                                a.Longitude,
                                RelatedPlaces = string.Join(" ", a.Tourist_Places.Select(p => p.Name)),
                                RelatedTours = string.Join(" ", a.Tours.Select(t => t.Name))
                            })
                            .ToListAsync();

                        if (!string.IsNullOrEmpty(keyword))
                        {
                            string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower();

                            var scoredAreas = allAreas.Select(a =>
                            {
                                string unSignName = StringHelper.ConvertToUnSign(a.Name ?? "").ToLower();
                                string unSignTitle = StringHelper.ConvertToUnSign(a.Title ?? "").ToLower();
                                string unSignAddress = StringHelper.ConvertToUnSign(a.Address ?? "").ToLower();
                                string unSignPlaces = StringHelper.ConvertToUnSign(a.RelatedPlaces ?? "").ToLower();
                                string unSignTours = StringHelper.ConvertToUnSign(a.RelatedTours ?? "").ToLower();

                                bool isExactMatch = unSignName.Contains(unSignKeyword) ||
                                                    unSignTitle.Contains(unSignKeyword) ||
                                                    unSignAddress.Contains(unSignKeyword) ||
                                                    unSignPlaces.Contains(unSignKeyword) ||
                                                    unSignTours.Contains(unSignKeyword);

                                // Lấy tỉ lệ giống cao nhất trong các trường
                                double maxSimilarity = new[] {
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignName),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignTitle),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignAddress),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignPlaces),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignTours)
                                }.Max();

                                // Chứa từ khóa thì max điểm 1.0, không thì lấy điểm tìm mềm
                                double finalScore = isExactMatch ? 1.0 : maxSimilarity;

                                return new { Item = a, Score = finalScore };
                            })
                            .Where(x => x.Score >= 0.5)
                            .OrderByDescending(x => x.Score) // Lên đỉnh nhờ độ chuẩn xác
                            .ThenByDescending(x => x.Item.RatingAverage) // Bằng điểm nhau thì xét Rating
                            .ToList();

                            allAreas = scoredAreas.Select(x => x.Item).ToList();
                        }
                        else
                        {
                            // Không gõ keyword thì cứ Rating cao lên đầu
                            allAreas = allAreas.OrderByDescending(a => a.RatingAverage).ToList();
                        }

                        var totalAreas = allAreas.Count;
                        var pagedAreas = allAreas.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToList();

                        var areaIds = pagedAreas.Select(a => a.Id).ToList();
                        var images = await _context.Imgs.Where(img => img.EntityType == "TouristArea" && areaIds.Contains(img.EntityId)).ToListAsync();

                        var areasResult = pagedAreas.Select(a => new
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
                        }).ToList();

                        return new { Items = areasResult, TotalCount = totalAreas, CurrentPage = req.Page };
                    }

                case "touristplace":
                    {
                        var placeQuery = _context.TouristPlaces.AsQueryable();

                        var allPlaces = await placeQuery
                            .Select(p => new
                            {
                                p.Id,
                                p.Name,
                                p.Title,
                                p.Address,
                                p.Description,
                                p.RatingAverage,
                                p.ClickCount,
                                p.FavoriteCount,
                                p.Latitude,
                                p.Longitude,
                                AreaName = p.Tourist_Area != null ? p.Tourist_Area.Name : "",
                                RelatedHotels = string.Join(" ", p.Hotels.Select(h => h.Name)),
                                RelatedTours = string.Join(" ", p.Tour_Itineraries.Where(ti => ti.Tour != null).Select(ti => ti.Tour!.Name))
                            })
                            .ToListAsync();

                        if (!string.IsNullOrEmpty(keyword))
                        {
                            string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower();

                            var scoredPlaces = allPlaces.Select(p =>
                            {
                                string unSignName = StringHelper.ConvertToUnSign(p.Name ?? "").ToLower();
                                string unSignTitle = StringHelper.ConvertToUnSign(p.Title ?? "").ToLower();
                                string unSignAddress = StringHelper.ConvertToUnSign(p.Address ?? "").ToLower();
                                string unSignArea = StringHelper.ConvertToUnSign(p.AreaName ?? "").ToLower();
                                string unSignHotels = StringHelper.ConvertToUnSign(p.RelatedHotels ?? "").ToLower();
                                string unSignTours = StringHelper.ConvertToUnSign(p.RelatedTours ?? "").ToLower();

                                bool isExactMatch = unSignName.Contains(unSignKeyword) || unSignTitle.Contains(unSignKeyword) ||
                                                    unSignAddress.Contains(unSignKeyword) || unSignArea.Contains(unSignKeyword) ||
                                                    unSignHotels.Contains(unSignKeyword) || unSignTours.Contains(unSignKeyword);

                                double maxSimilarity = new[] {
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignName),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignTitle),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignAddress),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignArea),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignHotels),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignTours)
                                }.Max();

                                double finalScore = isExactMatch ? 1.0 : maxSimilarity;
                                return new { Item = p, Score = finalScore };
                            })
                            .Where(x => x.Score >= 0.5)
                            .OrderByDescending(x => x.Score)
                            .ThenByDescending(x => x.Item.RatingAverage)
                            .ToList();

                            allPlaces = scoredPlaces.Select(x => x.Item).ToList();
                        }
                        else
                        {
                            allPlaces = allPlaces.OrderByDescending(p => p.RatingAverage).ToList();
                        }

                        var totalPlaces = allPlaces.Count;
                        var pagedPlaces = allPlaces.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToList();

                        var placeIds = pagedPlaces.Select(p => p.Id).ToList();
                        var imagesTouristPlace = await _context.Imgs.Where(img => img.EntityType == "TouristPlace" && placeIds.Contains(img.EntityId)).ToListAsync();

                        var placesResult = pagedPlaces.Select(a => new
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
                        }).ToList();

                        return new { Items = placesResult, TotalCount = totalPlaces, CurrentPage = req.Page };
                    }

                case "tour":
                    {
                        var tourQuery = _context.Tours.Where(t => t.Status == "ACTIVE" || t.Status == "Approved").AsNoTracking().AsSplitQuery().AsQueryable();

                        if (req.MinPrice.HasValue) tourQuery = tourQuery.Where(t => t.Price >= req.MinPrice.Value);
                        if (req.MaxPrice.HasValue) tourQuery = tourQuery.Where(t => t.Price <= req.MaxPrice.Value);
                        if (!string.IsNullOrEmpty(req.Category)) tourQuery = tourQuery.Where(t => t.TourType == req.Category);

                        var allTours = await tourQuery
                            .Select(t => new
                            {
                                t.Id,
                                t.Name,
                                t.Title,
                                t.Description,
                                t.DurationDays,
                                t.Price,
                                t.Vehicle,
                                t.TourType,
                                t.Status,
                                t.DepartureLocationName,
                                t.DepartureLatitude,
                                t.DepartureLongitude,
                                t.RatingAverage,
                                t.ClickCount,
                                t.FavoriteCount,
                                AreaName = t.Tourist_Area != null ? t.Tourist_Area.Name : "",
                                RelatedPlaces = string.Join(" ", t.Tour_Itinerarys.Where(ti => ti.Tourist_Place != null).Select(ti => ti.Tourist_Place!.Name))
                            })
                            .ToListAsync();

                        if (!string.IsNullOrEmpty(keyword))
                        {
                            string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower();

                            var scoredTours = allTours.Select(t =>
                            {
                                string unSignName = StringHelper.ConvertToUnSign(t.Name ?? "").ToLower();
                                string unSignTitle = StringHelper.ConvertToUnSign(t.Title ?? "").ToLower();
                                string unSignDeparture = StringHelper.ConvertToUnSign(t.DepartureLocationName ?? "").ToLower();
                                string unSignArea = StringHelper.ConvertToUnSign(t.AreaName ?? "").ToLower();
                                string unSignPlaces = StringHelper.ConvertToUnSign(t.RelatedPlaces ?? "").ToLower();

                                bool isExactMatch = unSignName.Contains(unSignKeyword) || unSignTitle.Contains(unSignKeyword) ||
                                                    unSignDeparture.Contains(unSignKeyword) || unSignArea.Contains(unSignKeyword) ||
                                                    unSignPlaces.Contains(unSignKeyword);

                                double maxSimilarity = new[] {
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignName),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignTitle),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignDeparture),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignArea),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignPlaces)
                                }.Max();

                                double finalScore = isExactMatch ? 1.0 : maxSimilarity;
                                return new { Item = t, Score = finalScore };
                            })
                            .Where(x => x.Score >= 0.5)
                            .OrderByDescending(x => x.Score)
                            .ThenByDescending(x => x.Item.RatingAverage)
                            .ToList();

                            allTours = scoredTours.Select(x => x.Item).ToList();
                        }
                        else
                        {
                            allTours = allTours.OrderByDescending(t => t.RatingAverage).ToList();
                        }

                        var totalTours = allTours.Count;
                        var pagedTours = allTours.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToList();

                        var tourIds = pagedTours.Select(t => t.Id).ToList();
                        var images = await _context.Imgs.Where(img => img.EntityType == "Tour" && tourIds.Contains(img.EntityId)).ToListAsync();

                        var toursResult = pagedTours.Select(a => new
                        {
                            id = a.Id,
                            name = a.Name,
                            title = a.Title,
                            description = a.Description,
                            durationDays = a.DurationDays,
                            price = a.Price,
                            vehicle = a.Vehicle,
                            tourType = a.TourType,
                            status = a.Status,
                            departure = new { name = a.DepartureLocationName, coords = new[] { a.DepartureLatitude, a.DepartureLongitude } },
                            rating_average = a.RatingAverage,
                            click_count = a.ClickCount,
                            favorite_count = a.FavoriteCount,
                            trending_Score = Math.Round((a.RatingAverage * 10m) + (a.FavoriteCount * 2m) + (a.ClickCount * 0.1m), 2),
                            type = "tour",
                            images = images.Where(img => img.EntityId == a.Id).ToList(),
                            coverImageUrl = images.FirstOrDefault(img => img.EntityId == a.Id && img.IsCover)?.url ?? "/Img/ImgNull.jpg"
                        }).ToList();

                        return new { Items = toursResult, TotalCount = totalTours, CurrentPage = req.Page };
                    }

                case "hotel":
                    {
                        var hotelQuery = _context.Hotels.Where(h => h.Status == "Active" || h.Status == "Approved").AsQueryable();

                        if (req.MinPrice.HasValue) hotelQuery = hotelQuery.Where(h => h.Price >= req.MinPrice.Value);
                        if (req.MaxPrice.HasValue) hotelQuery = hotelQuery.Where(h => h.Price <= req.MaxPrice.Value);

                        var allHotels = await hotelQuery
                            .Select(h => new
                            {
                                h.Id,
                                h.Name,
                                h.Title,
                                h.Address,
                                h.Description,
                                h.Price,
                                h.RatingAverage,
                                h.ClickCount,
                                h.FavoriteCount,
                                h.Latitude,
                                h.Longitude,
                                PlaceName = h.Tourist_Place != null ? h.Tourist_Place.Name : "",
                                AreaName = h.Tourist_Place != null && h.Tourist_Place.Tourist_Area != null ? h.Tourist_Place.Tourist_Area.Name : ""
                            })
                            .ToListAsync();

                        if (!string.IsNullOrEmpty(keyword))
                        {
                            string unSignKeyword = StringHelper.ConvertToUnSign(keyword).ToLower().Trim();

                            var scoredHotels = allHotels.Select(h =>
                            {
                                string unSignName = StringHelper.ConvertToUnSign(h.Name ?? "").ToLower();
                                string unSignTitle = StringHelper.ConvertToUnSign(h.Title ?? "").ToLower();
                                string unSignAddress = StringHelper.ConvertToUnSign(h.Address ?? "").ToLower();
                                string unSignArea = StringHelper.ConvertToUnSign(h.AreaName ?? "").ToLower();
                                string unSignPlace = StringHelper.ConvertToUnSign(h.PlaceName ?? "").ToLower();

                                bool isExactMatch = unSignName.Contains(unSignKeyword) || unSignTitle.Contains(unSignKeyword) ||
                                                    unSignAddress.Contains(unSignKeyword) || unSignArea.Contains(unSignKeyword) ||
                                                    unSignPlace.Contains(unSignKeyword);

                                double maxSimilarity = new[] {
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignName),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignTitle),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignAddress),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignArea),
                                    StringHelper.CalculateSimilarity(unSignKeyword, unSignPlace)
                                }.Max();

                                double finalScore = isExactMatch ? 1.0 : maxSimilarity;
                                return new { Item = h, Score = finalScore };
                            })
                            .Where(x => x.Score >= 0.5)
                            .OrderByDescending(x => x.Score) // Lên đỉnh nhờ độ chuẩn xác
                            .ThenByDescending(x => x.Item.RatingAverage) // Bằng điểm nhau thì xét Rating
                            .ToList();

                            allHotels = scoredHotels.Select(x => x.Item).ToList();
                        }
                        else
                        {
                            allHotels = allHotels.OrderByDescending(h => h.RatingAverage).ToList();
                        }

                        var totalHotels = allHotels.Count;
                        var pagedHotels = allHotels.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToList();

                        var hotelIds = pagedHotels.Select(h => h.Id).ToList();
                        var images = await _context.Imgs.Where(img => img.EntityType == "Hotel" && hotelIds.Contains(img.EntityId)).ToListAsync();

                        var hotelsResult = pagedHotels.Select(a => new
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
                        }).ToList();

                        return new { Items = hotelsResult, TotalCount = totalHotels, CurrentPage = req.Page };
                    }

                default:
                    throw new BadRequestException("Type không hợp lệ! Vui lòng chọn 'tour' hoặc 'hotel'.");
            }
        }
    }
}