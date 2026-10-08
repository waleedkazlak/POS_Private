using POS_Private.Application.Commands.Vendor;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Vendor;
using POS_Private.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search and list vendors with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<VendorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendors(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _mediator.Send(new GetVendorsQuery(searchTerm, isActive, pageNumber, pageSize));
        return Ok(response);
    }

    /// <summary>
    /// Get vendor by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<VendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendorById(int id)
    {
        var response = await _mediator.Send(new GetVendorByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Create new vendor
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
    {
        var response = await _mediator.Send(new CreateVendorCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Update existing vendor
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateVendor(int id, [FromBody] UpdateVendorDto dto)
    {
        var response = await _mediator.Send(new UpdateVendorCommand(id, dto));
        return Ok(response);
    }

    /// <summary>
    /// Delete vendor
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        var response = await _mediator.Send(new DeleteVendorCommand(id));
        return Ok(response);
    }

    /// <summary>
    /// Get Vendor Statement of Account (كشف حساب مورد) with running balance and transactions
    /// </summary>
    [HttpGet("{id:int}/statement")]
    [ProducesResponseType(typeof(APIBaseResponse<VendorStatementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendorStatement(
        int id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var response = await _mediator.Send(new GetVendorStatementQuery(id, fromDate, toDate));
        return Ok(response);
    }

    /// <summary>
    /// Search vendor purchase bills
    /// </summary>
    [HttpGet("bills")]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<VendorBillDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendorBills(
        [FromQuery] string? searchTerm,
        [FromQuery] int? vendorId,
        [FromQuery] int? branchId,
        [FromQuery] VendorBillStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _mediator.Send(new GetVendorBillsQuery(searchTerm, vendorId, branchId, status, fromDate, toDate, pageNumber, pageSize));
        return Ok(response);
    }

    /// <summary>
    /// Get vendor bill details by ID
    /// </summary>
    [HttpGet("bills/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<VendorBillDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendorBillById(int id)
    {
        var response = await _mediator.Send(new GetVendorBillByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Record a purchase bill from vendor (and optionally restock inventory)
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPost("bills")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateVendorBill([FromBody] CreateVendorBillDto dto)
    {
        var response = await _mediator.Send(new CreateVendorBillCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Get payment history for vendor
    /// </summary>
    [HttpGet("{id:int}/payments")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<VendorPaymentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVendorPayments(int id)
    {
        var response = await _mediator.Send(new GetVendorPaymentsQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Make payment to vendor (reduces vendor balance and updates ledger)
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager")]
    [HttpPost("payments")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateVendorPayment([FromBody] CreateVendorPaymentDto dto)
    {
        var response = await _mediator.Send(new CreateVendorPaymentCommand(dto));
        return Ok(response);
    }
}
