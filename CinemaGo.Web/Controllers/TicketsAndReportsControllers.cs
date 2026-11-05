using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

[Authorize]
public class TicketsController : BaseApiController
{
    private readonly ITicketService _tickets;

    public TicketsController(ITicketService tickets) => _tickets = tickets;

    // ბილეთის დეტალები. მომხმარებლები მხოლოდ საკუთარ ბილეთებს ხედავენ; ადმინისტრატორებს შეუძლიათ ყველა ბილეთის ნახვა.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _tickets.GetByIdAsync(id, CurrentUserId, IsAdmin, ct));
}

[Authorize(Roles = RoleNames.Admin)]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    // gayiduli ბილეთების რაოდენობა და შემოსავალი ფილმების მიხედვით (GroupBy / Count / Sum). სურვილისამებრ გადაეეცემა სეანსის დაწყების თარიღის დიაპაზონიც
    [HttpGet("revenue")]
    public async Task<ActionResult<IReadOnlyList<MovieRevenueResponse>>> Revenue(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _reports.GetRevenueByMovieAsync(from, to, ct));
}
