using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers
{
    /// <summary>
    /// დარბაზების კონტროლერი. პასუხისმგებელია კინოთეატრის დარბაზების, 
    /// მათი ადგილების განლაგებისა და სტრუქტურის მართვაზე.
    /// </summary>
    public class HallsController : BaseApiController
    {
        private readonly IHallService _halls;

        /// <summary>
        /// ინიციალიზაციას უკეთებს HallsController კლასის ახალ ეგზემპლარს.
        /// </summary>
        /// <param name="halls">დარბაზების სერვისი.</param>
        public HallsController(IHallService halls) => _halls = halls;

        /// <summary>აბრუნებს ყველა დარბაზის სიას მათი ადგილების რაოდენობასთან ერთად.</summary>
        /// <response code="200">დარბაზების სია.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IReadOnlyList<HallResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<HallResponse>>> GetAll(CancellationToken ct)
            => Ok(await _halls.GetAllAsync(ct));

        /// <summary>აბრუნებს კონკრეტული დარბაზის დეტალებს.</summary>
        /// <param name="id">დარბაზის იდენტიფიკატორი.</param>
        /// <response code="200">დარბაზი.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე დარბაზი არ მოიძებნა.</response>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HallResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HallResponse>> GetById(int id, CancellationToken ct)
            => Ok(await _halls.GetByIdAsync(id, ct));

        /// <summary>აბრუნებს დარბაზის ადგილების განლაგებას (რიგი, ნომერი, VIP დროშა).</summary>
        /// <param name="id">დარბაზის იდენტიფიკატორი.</param>
        /// <response code="200">ადგილები დალაგებული რიგებისა და ნომრების მიხედვით.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე დარბაზი არ მოიძებნა.</response>
        [HttpGet("{id:int}/seats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IReadOnlyList<SeatResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<SeatResponse>>> GetSeats(int id, CancellationToken ct)
            => Ok(await _halls.GetSeatsAsync(id, ct));

        /// <summary>ქმნის დარბაზს და ავტომატურად გენერირებს მის ადგილებს (რიგები x ადგილები რიგში) ერთ ნაბიჯში (მხოლოდ ადმინისტრატორისთვის).</summary>
        /// <response code="201">შექმნილი დარბაზი.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა (სახელი, რიგები, ადგილების რაოდენობა რიგში ან პირველი VIP რიგი).</response>
        /// <response code="409">ამ სახელით დარბაზი უკვე არსებობს.</response>
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(HallResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<HallResponse>> Create([FromBody] HallCreateRequest request, CancellationToken ct)
        {
            var created = await _halls.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>იცვლის დარბაზის სახელს (მხოლოდ ადმინისტრატორისთვის). ადგილების განლაგება არ იცვლება.</summary>
        /// <param name="id">დარბაზის იდენტიფიკატორი.</param>
        /// <response code="200">განახლებული დარბაზი.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე დარბაზი არ მოიძებნა.</response>
        /// <response code="409">სხვა დარბაზს უკვე აქვს ეს სახელი.</response>
        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(HallResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<HallResponse>> Update(int id, [FromBody] HallUpdateRequest request, CancellationToken ct)
            => Ok(await _halls.UpdateAsync(id, request, ct));

        /// <summary>ნიშნავს ადგილს VIP-ად ან სტანდარტულად (მხოლოდ ადმინისტრატორისთვის). გავლენას ახდენს მომავალი ჯავშნების ფასზე.</summary>
        /// <param name="id">დარბაზის იდენტიფიკატორი.</param>
        /// <param name="seatId">ადგილის იდენტიფიკატორი (უნდა ეკუთვნოდეს ამ დარბაზს).</param>
        /// <response code="200">განახლებული ადგილი.</response>
        /// <response code="404">დარბაზი ან ამ დარბაზში არსებული ადგილი არ მოიძებნა.</response>
        [HttpPut("{id:int}/seats/{seatId:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(SeatResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SeatResponse>> UpdateSeat(int id, int seatId, [FromBody] SeatUpdateRequest request, CancellationToken ct)
            => Ok(await _halls.UpdateSeatAsync(id, seatId, request, ct));

        /// <summary>შლის დარბაზს მის ადგილებთან ერთად (მხოლოდ ადმინისტრატორისთვის). შესაძლებელია მხოლოდ მაშინ, როცა დარბაზს არ აქვს აქტიური სეანსები.</summary>
        /// <param name="id">დარბაზის იდენტიფიკატორი.</param>
        /// <response code="204">წარმატებით წაიშალა.</response>
        /// <response code="404">ამ იდენტიფიკატორის მქონე დარბაზი არ მოიძებნა.</response>
        /// <response code="409">დარბაზს გააჩნია სეანსები.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _halls.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}