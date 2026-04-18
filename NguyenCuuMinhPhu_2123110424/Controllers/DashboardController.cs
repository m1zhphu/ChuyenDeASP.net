using Microsoft.AspNetCore.Mvc;
using SmartGarage.Services;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dbService;
    public DashboardController(DashboardService dbService) => _dbService = dbService;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() => Ok(await _dbService.GetStatsAsync());
}