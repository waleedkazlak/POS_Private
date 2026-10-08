using POS_Private.Application.Commands.Product;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Product;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search and paginate products catalog
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductSearchFilterDto filter)
    {
        var response = await _mediator.Send(new GetProductsQuery(filter));
        return Ok(response);
    }

    /// <summary>
    /// Get product details by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductById(int id)
    {
        var response = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(response);
    }

    /// <summary>
    /// Find product by Barcode or SKU
    /// </summary>
    [HttpGet("lookup/{barcodeOrSku}")]
    [ProducesResponseType(typeof(APIBaseResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductByBarcode(string barcodeOrSku)
    {
        var response = await _mediator.Send(new GetProductByBarcodeQuery(barcodeOrSku));
        return Ok(response);
    }

    /// <summary>
    /// Fast POS product catalog by branch/category
    /// </summary>
    [HttpGet("pos-catalog")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<PosProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosCatalog([FromQuery] int? branchId, [FromQuery] int? categoryId, [FromQuery] string? searchTerm)
    {
        var response = await _mediator.Send(new GetPosCatalogQuery(branchId, categoryId, searchTerm));
        return Ok(response);
    }

    /// <summary>
    /// Create new product (Admin / Inventory Manager)
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPost]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var response = await _mediator.Send(new CreateProductCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Update product
    /// </summary>
    [Authorize(Roles = "Admin,StoreManager,InventoryManager")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        dto.Id = id;
        var response = await _mediator.Send(new UpdateProductCommand(dto));
        return Ok(response);
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var response = await _mediator.Send(new DeleteProductCommand(id));
        return Ok(response);
    }
}
