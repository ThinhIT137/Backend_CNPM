using backend.DTO;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("/api/[controller]")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [AllowAnonymous]
        [HttpPost("filter")]
        public async Task<IActionResult> FilterData([FromBody] SearchFilterRequest req)
        {
            var data = await _searchService.FilterAsync(req);
            return Ok(new { success = true, message = "Lọc dữ liệu thành công", data = data });
        }
    }
}
