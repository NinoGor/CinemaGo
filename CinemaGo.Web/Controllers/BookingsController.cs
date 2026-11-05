using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

[Authorize]
public class BookingsController : BaseApiController
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings) => _bookings = bookings;

    // ერთ ტრანზაქციაში რამდენიმე ადგილის დაჯავშნა; თითოეულ ადგილზე ერთი ჯავშანი + ბილეთი იქმნება
    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var created = await _bookings.CreateAsync(CurrentUserId, request, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    // გამომძახებლის ჯავშნის გაუქმება
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BookingResponse>> Cancel(int id, CancellationToken ct)
        => Ok(await _bookings.CancelAsync(CurrentUserId, id, ct));

    // გამომძახებლისჯავშნების ისტორია (თარიღის კლებადობით)
    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetMy([FromQuery] PaginationParams paging, CancellationToken ct)
        => Ok(await _bookings.GetMyBookingsAsync(CurrentUserId, paging, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _bookings.GetByIdAsync(CurrentUserId, id, ct));
}
