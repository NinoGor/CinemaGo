using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers
{
    /// <summary>
    /// ბილეთების კონტროლერი. პასუხისმგებელია ბილეთის დეტალური ინფორმაციის მიღებაზე 
    /// ავტორიზებული მომხმარებლებისთვის და ადმინისტრატორებისთვის.
    /// </summary>
    [Authorize]
    public class TicketsController : BaseApiController
    {
        private readonly ITicketService _tickets;

        /// <summary>
        /// ინიციალიზაციას უკეთებს TicketsController კლასის ახალ ეგზემპლარს.
        /// </summary>
        /// <param name="tickets">ბილეთების სერვისი.</param>
        public TicketsController(ITicketService tickets) => _tickets = tickets;

        /// <summary>ბილეთის დეტალები. მომხმარებლებს შეუძლიათ მხოლოდ საკუთარი ბილეთების ნახვა; ადმინისტრატორებს - ყველასი.</summary>
        /// <param name="id">ბილეთის იდენტიფიკატორი.</param>
        /// <response code="200">ბილეთი.</response>
        /// <response code="403">ბილეთი სხვა პირს ეკუთვნის.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე ბილეთი არ მოიძებნა.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TicketResponse>> GetById(int id, CancellationToken ct)
            => Ok(await _tickets.GetByIdAsync(id, CurrentUserId, IsAdmin, ct));
    }

    /// <summary>
    /// ანგარიშების (Reports) კონტროლერი. განკუთვნილია მხოლოდ ადმინისტრატორებისთვის 
    /// ფინანსური და სტატისტიკური მონაცემების მისაღებად.
    /// </summary>
    [Authorize(Roles = RoleNames.Admin)]
    public class ReportsController : BaseApiController
    {
        private readonly IReportService _reports;

        /// <summary>
        /// ინიციალიზაციას უკეთებს ReportsController კლასის ახალ ეგზემპლარს.
        /// </summary>
        /// <param name="reports">ანგარიშების სერვისი.</param>
        public ReportsController(IReportService reports) => _reports = reports;

        /// <summary>გაყიდული ბილეთები და შემოსავალი ფილმების მიხედვით (GroupBy / Count / Sum). სეანსის დაწყების არასავალდებულო თარიღების დიაპაზონი.</summary>
        /// <param name="from">მხოლოდ ის სეანსები, რომლებიც იწყება ამ UTC დროზე ან მის შემდეგ.</param>
        /// <param name="to">მხოლოდ ის სეანსები, რომლებიც იწყება ამ UTC დრომდე ან ზუსტად ამ დროს.</param>
        /// <response code="200">თითო სტრიქონი თითო ფილმზე, ყველაზე გაყიდვადი პირველ ადგილზე.</response>
        [HttpGet("revenue")]
        [ProducesResponseType(typeof(IReadOnlyList<MovieRevenueResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MovieRevenueResponse>>> Revenue(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
            => Ok(await _reports.GetRevenueByMovieAsync(from, to, ct));
    }
}