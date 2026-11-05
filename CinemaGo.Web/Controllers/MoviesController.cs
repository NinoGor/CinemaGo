using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

public class MoviesController : BaseApiController
{
    private readonly IMovieService _movies;

    public MoviesController(IMovieService movies) => _movies = movies;

    // ფილმების დათვალიერება და ფილტრაცია (სახელი, ჟანრი, ასაკობრივი შეფასება), სორტირებითა და პაგინაციით
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<MovieResponse>>> GetAll([FromQuery] MovieFilter filter, CancellationToken ct)
        => Ok(await _movies.GetPagedAsync(filter, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<MovieResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _movies.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<MovieResponse>> Create([FromBody] MovieRequest request, CancellationToken ct)
    {
        var created = await _movies.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<MovieResponse>> Update(int id, [FromBody] MovieRequest request, CancellationToken ct)
        => Ok(await _movies.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _movies.DeleteAsync(id, ct);
        return NoContent();
    }
}
