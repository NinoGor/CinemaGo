using CinemaGo.Application.Settings;
using CinemaGo.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    // ავთენტიფიცირებული მომხმარებლის Id JWT-დან 
    protected int CurrentUserId =>
        int.Parse(User.FindFirst(AppClaimTypes.UserId)?.Value
                  ?? throw new InvalidOperationException("The token has no user id claim."));

    protected bool IsAdmin => User.IsInRole(RoleNames.Admin);
}
