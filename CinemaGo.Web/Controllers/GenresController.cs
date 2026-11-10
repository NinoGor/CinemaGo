using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers
{
    /// <summary>
    /// ჟანრების კონტროლერი. პასუხისმგებელია ფილმების ჟანრების მართვაზე: 
    /// სიაფის მიღებაზე, შექმნაზე, განახლებასა და წაშლაზე.
    /// </summary>
    public class GenresController : BaseApiController
    {
        private readonly IGenreService _genres;

        /// <summary>
        /// ინიციალიზაციას უკეთებს GenresController კლასის ახალ ეგზემპლარს.
        /// </summary>
        /// <param name="genres">ჟანრების სერვისი.</param>
        public GenresController(IGenreService genres) => _genres = genres;

        /// <summary>აბრუნებს ყველა ჟანრის სიას, სახელების მიხედვით დალაგებულს.</summary>
        /// <response code="200">ჟანრების სია.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IReadOnlyList<GenreResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<GenreResponse>>> GetAll(CancellationToken ct)
            => Ok(await _genres.GetAllAsync(ct));

        /// <summary>აბრუნებს კონკრეტული ჟანრის დეტალებს.</summary>
        /// <param name="id">ჟანრის იდენტიფიკატორი.</param>
        /// <response code="200">ჟანრი.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე ჟანრი არ მოიძებნა.</response>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GenreResponse>> GetById(int id, CancellationToken ct)
            => Ok(await _genres.GetByIdAsync(id, ct));

        /// <summary>ქმნის ახალ ჟანრს (მხოლოდ ადმინისტრატორისთვის).</summary>
        /// <response code="201">შექმნილი ჟანრი.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა (ცარიელი ან ძალიან გრძელი სახელი).</response>
        /// <response code="409">ამ სახელით ჟანრი უკვე არსებობს.</response>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<GenreResponse>> Create([FromBody] GenreRequest request, CancellationToken ct)
        {
            var created = await _genres.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>იცვლის ჟანრის სახელს (მხოლოდ ადმინისტრატორისთვის).</summary>
        /// <param name="id">ჟანრის იდენტიფიკატორი.</param>
        /// <response code="200">განახლებული ჟანრი.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე ჟანრი არ მოიძებნა.</response>
        /// <response code="409">სხვა ჟანრს უკვე აქვს ეს სახელი.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(GenreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<GenreResponse>> Update(int id, [FromBody] GenreRequest request, CancellationToken ct)
            => Ok(await _genres.UpdateAsync(id, request, ct));

        /// <summary>შლის იმ ჟანრს, რომელსაც არცერთი ფილმი არ იყენებს (მხოლოდ ადმინისტრატორისთვის).</summary>
        /// <param name="id">ჟანრის იდენტიფიკატორი.</param>
        /// <response code="204">წარმატებით წაიშალა.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე ჟანრი არ მოიძებნა.</response>
        /// <response code="409">ჟანრი ჯერ კიდევ გამოიყენება ფილმების მიერ.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _genres.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}