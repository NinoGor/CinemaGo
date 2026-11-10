using CinemaGo.Application.Settings;
using CinemaGo.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers
{
    /// <summary>
    /// საბაზო API კონტროლერი, რომელიც შეიცავს საერთო დამხმარე მეთოდებსა და 
    /// თვისებებს (Properties) მომხმარებლის იდენტიფიკაციისა და როლების შესამოწმებლად.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// იღებს მიმდინარე ავთენტიფიცირებული მომხმარებლის უნიკალურ იდენტიფიკატორს (Id) JWT ტოკენიდან.
        /// </summary>
        protected int CurrentUserId =>
            int.Parse(User.FindFirst(AppClaimTypes.UserId)?.Value
                      ?? throw new InvalidOperationException("The token has no user id claim."));

        /// <summary>
        /// აბრუნებს ჭეშმარიტ (true) მნიშვნელობას, თუ მიმდინარე ავთენტიფიცირებული მომხმარებელი არის ადმინისტრატორი.
        /// </summary>
        protected bool IsAdmin => User.IsInRole(RoleNames.Admin);
    }
}
