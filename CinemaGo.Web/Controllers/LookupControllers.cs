using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

[AllowAnonymous]
public class GenresController : BaseApiController
{
    private readonly ILookupService _lookup;

    public GenresController(ILookupService lookup) => _lookup = lookup;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GenreResponse>>> GetAll(CancellationToken ct)
        => Ok(await _lookup.GetGenresAsync(ct));
}

[AllowAnonymous]
public class HallsController : BaseApiController
{
    private readonly ILookupService _lookup;

    public HallsController(ILookupService lookup) => _lookup = lookup;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HallResponse>>> GetAll(CancellationToken ct)
        => Ok(await _lookup.GetHallsAsync(ct));
}
