using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers
{
    /// <summary>
    /// ჯავშნების კონტროლერი. პასუხისმგებელია სეანსებზე ადგილების დაჯავშნაზე, 
    /// ჯავშნების ისტორიის ნახვასა და მათ გაუქმებაზე.
    /// </summary>
    [Authorize]
    public class BookingsController : BaseApiController
    {
        private readonly IBookingService _bookings;

        /// <summary>
        /// ინიციალიზაციას უკეთებს BookingsController კლასის ახალ ეგზემპლარს.
        /// </summary>
        /// <param name="bookings">ჯავშნების სერვისი.</param>
        public BookingsController(IBookingService bookings) => _bookings = bookings;

        /// <summary>ერთ ტრანზაქციაში რამდენიმე ადგილის დაჯავშნა; თითოეულ ადგილზე თითო ჯავშანი და ბილეთი იქმნება.</summary>
        /// <response code="201">შექმნილი ჯავშნები, თითო თითო ადგილისთვის.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა (ადგილები მითითებული არ არის, დუბლირებული ადგილები...).</response>
        /// <response code="401">აკლია ან არასწორია ტოკენი.</response>
        /// <response code="404">სეანსი ან ადგილი არ არსებობს.</response>
        /// <response code="409">ადგილი უკვე დაკავებულია (ამ შემთხვევაში არაფერი ჯავშნდება).</response>
        /// <response code="422">ადგილი სხვა დარბაზშია, ან სეანსი უკვე დაწყებულია/გაუქმებულია.</response>
        [HttpPost]
        [ProducesResponseType(typeof(IReadOnlyList<BookingResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
        {
            var created = await _bookings.CreateAsync(CurrentUserId, request, ct);
            return StatusCode(StatusCodes.Status201Created, created);
        }

        /// <summary>აუქმებს მომხმარებლის საკუთარ ერთ-ერთ ჯავშანს.</summary>
        /// <param name="id">ჯავშნის იდენტიფიკატორი.</param>
        /// <response code="200">გაუქმებული ჯავშანი.</response>
        /// <response code="403">ჯავშანი სხვა პირს ეკუთვნის.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე ჯავშანი არ მოიძებნა.</response>
        /// <response code="409">უკვე გაუქმებულია.</response>
        /// <response code="422">სეანსი უკვე დაწყებულია.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<BookingResponse>> Cancel(int id, CancellationToken ct)
            => Ok(await _bookings.CancelAsync(CurrentUserId, id, ct));

        /// <summary>მომხმარებლის ჯავშნების ისტორია (უახლესი პირველრიგში).</summary>
        /// <response code="200">მომხმარებლის ჯავშნების გვერდი.</response>
        [HttpGet("my")]
        [ProducesResponseType(typeof(PagedResult<BookingResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<BookingResponse>>> GetMy([FromQuery] PaginationParams paging, CancellationToken ct)
            => Ok(await _bookings.GetMyBookingsAsync(CurrentUserId, paging, ct));

        /// <summary>აბრუნებს მომხმარებლის საკუთარი ჯავშნის დეტალებს.</summary>
        /// <param name="id">ჯავშნის იდენტიფიკატორი.</param>
        /// <response code="200">ჯავშანი.</response>
        /// <response code="403">ჯავშანი სხვა პირს ეკუთვნის.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე ჯავშანი არ მოიძებნა.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingResponse>> GetById(int id, CancellationToken ct)
            => Ok(await _bookings.GetByIdAsync(CurrentUserId, id, ct));
    }
}