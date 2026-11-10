using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

/// <summary>
/// ფილმების კონტროლერი. პასუხისმგებელია ფილმების კატალოგის მართვაზე, 
/// ძიებაზე, ფილტრაციაზე, პოსტერების ატვირთვასა და წაშლაზე.
/// </summary>
public class MoviesController : BaseApiController
{
    private const long PosterRequestLimitBytes = 3 * 1024 * 1024;

    private readonly IMovieService _movies;

    /// <summary>
    /// ინიციალიზაციას უკეთებს MoviesController კლასის ახალ ეგზემპლარს.
    /// </summary>
    /// <param name="movies">ფილმების სერვისი.</param>
    public MoviesController(IMovieService movies) => _movies = movies;

    /// <summary>ფილმების ძიება და ფილტრაცია (სათაური, ჟანრი, ასაკობრივი ზღვარი) სორტირებითა და პაგინაციით.</summary>
    /// <response code="200">ფილმების გვერდი.</response>
    /// <response code="400">არასწორი ფილტრი ან პაგინაციის მნიშვნელობები.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<MovieResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<MovieResponse>>> GetAll([FromQuery] MovieFilter filter, CancellationToken ct)
        => Ok(await _movies.GetPagedAsync(filter, ct));

    /// <summary>აბრუნებს კონკრეტულ ფილმს მისი ჟანრებითა და პოსტერის ბმულით.</summary>
    /// <param name="id">ფილმის იდენტიფიკატორი.</param>
    /// <response code="200">ფილმი.</response>
    /// <response code="404">ამ იდენტიფიკატორის მქონე ფილმი არ მოიძებნა.</response>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _movies.GetByIdAsync(id, ct));

    /// <summary>ქმნის ფილმს (მხოლოდ ადმინისტრატორისთვის). PosterUrl არ არის სავალდებულო: შეიძლება იყოს http(s) ბმული ან ისეთი გზა, როგორიცაა /posters/x.jpg.</summary>
    /// <response code="201">შექმნილი ფილმი.</response>
    /// <response code="400">ვალიდაცია ვერ გაიარა.</response>
    /// <response code="422">ერთ-ერთი ჟანრის იდენტიფიკატორი არ არსებობს.</response>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MovieResponse>> Create([FromBody] MovieRequest request, CancellationToken ct)
    {
        var created = await _movies.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>ანაცვლებს ფილმის მონაცემებს (მხოლოდ ადმინისტრატორისთვის).</summary>
    /// <param name="id">ფილმის იდენტიფიკატორი.</param>
    /// <response code="200">განახლებული ფილმი.</response>
    /// <response code="400">ვალიდაცია ვერ გაიარა.</response>
    /// <response code="404">ამ იდენტიფიკატორის მქონე ფილმი არ მოიძებნა.</response>
    /// <response code="422">ერთ-ერთი ჟანრის იდენტიფიკატორი არ არსებობს.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MovieResponse>> Update(int id, [FromBody] MovieRequest request, CancellationToken ct)
        => Ok(await _movies.UpdateAsync(id, request, ct));

    /// <summary>ტვირთავს ფილმის პოსტერის სურათს (მხოლოდ ადმინისტრატორისთვის). JPEG, PNG ან WebP, 2 მბ-მდე; ანაცვლებს მიმდინარე პოსტერს.</summary>
    /// <param name="id">ფილმის იდენტიფიკატორი.</param>
    /// <param name="file">სურათი, რომელიც იგზავნება როგორც multipart/form-data ველი "file".</param>
    /// <response code="200">ფილმი ახალი პოსტერის მისამართით.</response>
    /// <response code="400">ფაილი არ გამოგზავნილა.</response>
    /// <response code="404">ამ იდენტიფიკატორის მქონე ფილმი არ მოიძებნა.</response>
    /// <response code="413">მოთხოვნა დასაშვებზე დიდია.</response>
    /// <response code="422">ფაილი ცარიელია, ძალიან დიდია, ან არ არის JPEG/PNG/WebP სურათი.</response>
    [HttpPost("{id:int}/poster")]
    [Authorize(Roles = RoleNames.Admin)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PosterRequestLimitBytes)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<MovieResponse>> UploadPoster(int id, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await _movies.UploadPosterAsync(id, stream, ct));
    }

    /// <summary> აყენებს პოსტერს ბმულიდან ფაილის ნაცვლად (მხოლოდ ადმინისტრატორისთვის). ცარიელი მნიშვნელობა შლის პოსტერს.</summary>
    /// <param name="id">ფილმის იდენტიფიკატორი.</param>
    /// <response code="200">განახლებული ფილმი.</response>
    /// <response code="400">ბმული არ არის http(s) URL ან არ იწყება '/' სიმბოლოთი.</response>
    /// <response code="404">ამ იდენტიფიკატორის მქონე ფილმი არ მოიძებნა.</response>
    [HttpPut("{id:int}/poster")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> SetPoster(int id, [FromBody] PosterUrlRequest request, CancellationToken ct)
        => Ok(await _movies.SetPosterAsync(id, request.PosterUrl, ct));

    /// <summary>შლის ფილმის პოსტერს (მხოლოდ ადმინისტრატორისთვის). ატვირთული ფაილი იშლება დისკიდან.</summary>
    /// <param name="id">ფილმის იდენტიფიკატორი.</param>
    /// <response code="200">ფილმი პოსტერის გარეშე.</response>
    /// <response code="404">ამ იდენტიფიკატორის მქონე ფილმი არ მოიძებნა.</response>
    [HttpDelete("{id:int}/poster")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> RemovePoster(int id, CancellationToken ct)
        => Ok(await _movies.RemovePosterAsync(id, ct));

    /// <summary>შლის იმ ფილმს, რომელსაც არ გააჩნია სეანსები (მხოლოდ ადმინისტრატორისთვის).</summary>
    /// <param name="id">ფილმის იდენტიფიკატორი.</param>
    /// <response code="204">წარმატებით წაიშალა.</response>
    /// <response code="404">ამ იდენტიფიკატორის მქონე ფილმი არ მოიძებნა.</response>
    /// <response code="409">ფილმს გააჩნია სეანსები.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _movies.DeleteAsync(id, ct);
        return NoContent();
    }
}