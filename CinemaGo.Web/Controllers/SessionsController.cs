using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

public class SessionsController : BaseApiController
{
    private readonly ISessionService _sessions;

    public SessionsController(ISessionService sessions) => _sessions = sessions;

    // ფილმების სეანსების დათვალიერება: ფილტრაცია ფილმის, დარბაზის, ჟანრისა და თარიღის დიაპაზონით; default-ად ნაჩვენებია მხოლოდ მომავალი სეანსები
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<SessionResponse>>> Search([FromQuery] SessionFilter filter, CancellationToken ct)
        => Ok(await _sessions.SearchAsync(filter, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _sessions.GetByIdAsync(id, ct));

    // ფილმის სეანსის თავისუფალი / დაკავებული ადგილები, თითოეული ადგილის ფასით
    [HttpGet("{id:int}/seats")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionSeatsResponse>> GetSeats(int id, CancellationToken ct)
        => Ok(await _sessions.GetSeatsAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<SessionResponse>> Create([FromBody] SessionRequest request, CancellationToken ct)
    {
        var created = await _sessions.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<SessionResponse>> Update(int id, [FromBody] SessionRequest request, CancellationToken ct)
        => Ok(await _sessions.UpdateAsync(id, request, ct));

    // სეანსის გაუქმება და აქტიური ჯავშნების გაუქმება (მომხმარებლები მიიღებენ შეტყობინებას სიმულირებული ელ-ფოსტით)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<SessionCancellationResponse>> Cancel(int id, CancellationToken ct)
        => Ok(await _sessions.CancelAsync(id, ct));
}
