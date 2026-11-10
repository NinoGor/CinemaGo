using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers
{
    /// <summary>
    /// სეანსების კონტროლერი. პასუხისმგებელია კინოთეატრის სეანსების ძიებაზე, 
    /// დაგეგმვაზე, განახლებაზე, გაუქმებაზე და სეანსის ადგილების მართვაზე.
    /// </summary>
    public class SessionsController : BaseApiController
    {
        private readonly ISessionService _sessions;

        /// <summary>
        /// ინიციალიზაციას უკეთებს SessionsController კლასის ახალ ეგზემპლარს.
        /// </summary>
        /// <param name="sessions">სეანსების სერვისი.</param>
        public SessionsController(ISessionService sessions) => _sessions = sessions;

        /// <summary>სეანსების ძიება: ფილტრაცია ფილმის, დარბაზის, ჟანრისა და თარიღების დიაპაზონის მიხედვით; სტანდარტულად აჩვენებს მხოლოდ მომავალ სეანსებს.</summary>
        /// <response code="200">სეანსების გვერდი.</response>
        /// <response code="400">არასწორი ფილტრი ან პაგინაციის მნიშვნელობები.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<SessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<SessionResponse>>> Search([FromQuery] SessionFilter filter, CancellationToken ct)
            => Ok(await _sessions.SearchAsync(filter, ct));

        /// <summary>აბრუნებს კონკრეტულ სეანსს მის ფილმთან, დარბაზთან, დროებასა და ფასებთან ერთად.</summary>
        /// <param name="id">სეანსის იდენტიფიკატორი.</param>
        /// <response code="200">სეანსი.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე სეანსი არ მოიძებნა.</response>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SessionResponse>> GetById(int id, CancellationToken ct)
            => Ok(await _sessions.GetByIdAsync(id, ct));

        /// <summary>სეანსის თავისუფალი / დაკავებული ადგილები თითოეული ადგილის ფასთან ერთად.</summary>
        /// <param name="id">სეანსის იდენტიფიკატორი.</param>
        /// <response code="200">დარბაზის ყველა ადგილი თავისი ფასით და დაკავებულობის დროშით (booked).</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე სეანსი არ მოიძებნა.</response>
        [HttpGet("{id:int}/seats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SessionSeatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SessionSeatsResponse>> GetSeats(int id, CancellationToken ct)
            => Ok(await _sessions.GetSeatsAsync(id, ct));

        /// <summary>გეგმავს სეანსს (მხოლოდ ადმინისტრატორისთვის). დასასრულის დრო არის დასაწყისი + ფილმის ხანგრძლივობა; ერთ დარბაზში სეანსები არ უნდა იკვეთებოდეს.</summary>
        /// <response code="201">შექმნილი სეანსი.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა.</response>
        /// <response code="404">ფილმი ან დარბაზი არ არსებობს.</response>
        /// <response code="409">დარბაზი ამ დროისთვის უკვე დაკავებულია.</response>
        /// <response code="422">დაწყების დრო წარსულშია.</response>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SessionResponse>> Create([FromBody] SessionRequest request, CancellationToken ct)
        {
            var created = await _sessions.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>არეგულირებს (თავიდან გეგმავს) ან ცვლის სეანსის ფასებს (მხოლოდ ადმინისტრატორისთვის).</summary>
        /// <param name="id">სეანსის იდენტიფიკატორი.</param>
        /// <response code="200">განახლებული სეანსი.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა.</response>
        /// <response code="404">სეანსი, ფილმი ან დარბაზი არ არსებობს.</response>
        /// <response code="409">დარბაზი ამ დროისთვის უკვე დაკავებულია, ან სეანსი გაუქმებულია / აქვს აქტიური ჯავშნები.</response>
        /// <response code="422">სეანსი უკვე დაწყებულია, ან ახალი დაწყების დრო წარსულშია.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SessionResponse>> Update(int id, [FromBody] SessionRequest request, CancellationToken ct)
            => Ok(await _sessions.UpdateAsync(id, request, ct));

        /// <summary>აუქმებს სეანსს და მასობრივად აუქმებს მის ყველა აქტიურ ჯავშანს (მომხმარებლები შეტყობინებას იღებენ სიმულირებული ელ-ფოსტით).</summary>
        /// <param name="id">სეანსის იდენტიფიკატორი.</param>
        /// <response code="200">რამდენი ჯავშანიც გაუქმდა.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე სეანსი არ მოიძებნა.</response>
        /// <response code="409">სეანსი უკვე გაუქმებულია.</response>
        /// <response code="422">სეანსი უკვე დაწყებულია.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(SessionCancellationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<SessionCancellationResponse>> Cancel(int id, CancellationToken ct)
            => Ok(await _sessions.CancelAsync(id, ct));
    }
}