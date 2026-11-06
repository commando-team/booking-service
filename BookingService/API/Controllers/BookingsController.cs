using API.Dto.Booking;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var id = await bookingService.CreateBooking(request.ResourceId, request.From, request.To);
        return CreatedAtRoute(nameof(GetBookingById), new {id = id}, id);
    }
    
    [HttpGet("{id:guid}", Name = nameof(GetBookingById))]
    public async Task<ActionResult<GetBookingResponse>> GetBookingById(Guid id)
    {
        var booking = await bookingService.GetBooking(id);
        return Ok(new GetBookingResponse(
            booking.Id,
            booking.ResourceId,
            booking.UserId,
            booking.From,
            booking.To,
            booking.Status,
            booking.Confirmed)
        );
    }
    
    [HttpPost("{id:guid}")]
    public async Task<IActionResult> ConfirmBooking(Guid id)
    {
        await bookingService.ConfirmBooking(id);
        return Ok();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        await bookingService.CancelBooking(id);
        return NoContent();
    }
}