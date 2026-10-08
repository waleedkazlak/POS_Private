using POS_Private.Application.Commands.Coupon;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Coupon;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all discount coupons
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<CouponDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoupons()
    {
        var response = await _mediator.Send(new GetCouponsQuery());
        return Ok(response);
    }

    /// <summary>
    /// Validate coupon code and calculate discount for order amount
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(APIBaseResponse<CouponValidationResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponDto dto)
    {
        var response = await _mediator.Send(new ValidateCouponQuery(dto));
        return Ok(response);
    }

    /// <summary>
    /// Create new coupon (Admin)
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager")]
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        var response = await _mediator.Send(new CreateCouponCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Delete coupon
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        var response = await _mediator.Send(new DeleteCouponCommand(id));
        return Ok(response);
    }
}
