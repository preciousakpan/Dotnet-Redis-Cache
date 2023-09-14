using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Redis_Cache.Data;
using Redis_Cache.Models;
using Redis_Cache.Services;

namespace Redis_Cache.Controllers
{
[ApiController]
[Route("[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly ILogger<DriverController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _context;

        public DriverController(ILogger<DriverController> logger, ICacheService cacheService, AppDbContext context)
        {
            _logger = logger;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> Get()
        {
            // Check Redis Cache For Data
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
            if (cacheData != null && cacheData.Count() > 0)
            {
                return Ok(cacheData);
            }
            cacheData = await _context.Drivers.ToListAsync();

            // Add data to cache, set expiry time
            var expiry = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiry);

            return Ok(cacheData);
        }

        [HttpPost("AddDriver")]
        public async Task<IActionResult> Post(Driver value)
        {
            var addedObj = await _context.Drivers.AddAsync(value);
            var expiry = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<Driver>($"driver{value.Id}", addedObj.Entity, expiry);

            await _context.SaveChangesAsync();
            return Ok(addedObj.Entity);

        }

        [HttpDelete("DeleteDriver")]
        public async Task<IActionResult> Delete (int id)
        {
            var exist = await _context.Drivers.FirstOrDefaultAsync(c => c.Id == id);

            if (exist != null)
            {
                _context.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _context.SaveChangesAsync();

                return NoContent();
            }
            return NotFound();
        }
    }
}