using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public LookupsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get list of all available product variant colors
    /// </summary>
    [HttpGet("colors")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<ColorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetColors([FromQuery] bool? includeInactive = false)
    {
        var colors = await _unitOfWork.Colors.GetAllAsync();
        var query = includeInactive == true ? colors : colors.Where(c => c.IsActive);
        var dtos = query.Select(c => new ColorDto
        {
            Id = c.Id,
            Name = LocalizationHelper.Localize(c.NameEn, c.NameAr),
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            HexCode = c.HexCode,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive
        });

        return Ok(APIBaseResponse<IEnumerable<ColorDto>>.Success(dtos));
    }

    /// <summary>
    /// Create new color lookup
    /// </summary>
    [HttpPost("colors")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateColor([FromBody] CreateColorDto dto)
    {
        var color = new POS_Private.Domain.Entities.Color
        {
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            HexCode = dto.HexCode,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        await _unitOfWork.Colors.AddAsync(color);
        await _unitOfWork.SaveChangesAsync();
        return Ok(APIBaseResponse<int>.Success(color.Id));
    }

    /// <summary>
    /// Update color lookup
    /// </summary>
    [HttpPut("colors/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateColor(int id, [FromBody] CreateColorDto dto)
    {
        var color = await _unitOfWork.Colors.GetByIdAsync(id);
        if (color == null)
            return NotFound(APIBaseResponse<bool>.Failed(StatusCodes.Status404NotFound, "Color not found"));

        color.NameEn = dto.NameEn;
        color.NameAr = dto.NameAr;
        color.HexCode = dto.HexCode;
        color.DisplayOrder = dto.DisplayOrder;
        color.IsActive = dto.IsActive;

        await _unitOfWork.Colors.UpdateAsync(color);
        await _unitOfWork.SaveChangesAsync();
        return Ok(APIBaseResponse<bool>.Success(true));
    }

    /// <summary>
    /// Delete color lookup
    /// </summary>
    [HttpDelete("colors/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteColor(int id)
    {
        await _unitOfWork.Colors.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return Ok(APIBaseResponse<bool>.Success(true));
    }

    /// <summary>
    /// Get list of all available product variant sizes
    /// </summary>
    [HttpGet("sizes")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<SizeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSizes([FromQuery] bool? includeInactive = false)
    {
        var sizes = await _unitOfWork.Sizes.GetAllAsync();
        var query = includeInactive == true ? sizes : sizes.Where(s => s.IsActive);
        var dtos = query.Select(s => new SizeDto
        {
            Id = s.Id,
            Name = LocalizationHelper.Localize(s.NameEn, s.NameAr),
            NameEn = s.NameEn,
            NameAr = s.NameAr,
            Code = s.Code,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive
        });

        return Ok(APIBaseResponse<IEnumerable<SizeDto>>.Success(dtos));
    }

    /// <summary>
    /// Create new size lookup
    /// </summary>
    [HttpPost("sizes")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSize([FromBody] CreateSizeDto dto)
    {
        var size = new POS_Private.Domain.Entities.Size
        {
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            Code = dto.Code,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        await _unitOfWork.Sizes.AddAsync(size);
        await _unitOfWork.SaveChangesAsync();
        return Ok(APIBaseResponse<int>.Success(size.Id));
    }

    /// <summary>
    /// Update size lookup
    /// </summary>
    [HttpPut("sizes/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSize(int id, [FromBody] CreateSizeDto dto)
    {
        var size = await _unitOfWork.Sizes.GetByIdAsync(id);
        if (size == null)
            return NotFound(APIBaseResponse<bool>.Failed(StatusCodes.Status404NotFound, "Size not found"));

        size.NameEn = dto.NameEn;
        size.NameAr = dto.NameAr;
        size.Code = dto.Code;
        size.DisplayOrder = dto.DisplayOrder;
        size.IsActive = dto.IsActive;

        await _unitOfWork.Sizes.UpdateAsync(size);
        await _unitOfWork.SaveChangesAsync();
        return Ok(APIBaseResponse<bool>.Success(true));
    }

    /// <summary>
    /// Delete size lookup
    /// </summary>
    [HttpDelete("sizes/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteSize(int id)
    {
        await _unitOfWork.Sizes.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return Ok(APIBaseResponse<bool>.Success(true));
    }
    /// <summary>
    /// Get list of supported payment methods (Cash, Visa, InstaPay, Vodafone Cash, etc.)
    /// </summary>
    [HttpGet("payment-methods")]
    [ProducesResponseType(typeof(APIBaseResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetPaymentMethods()
    {
        var isArabic = LocalizationHelper.IsArabic;
        var methods = Enum.GetValues<PaymentMethod>().Select(m => new
        {
            Id = (int)m,
            Name = m.ToString(),
            DisplayName = isArabic ? m switch
            {
                PaymentMethod.Cash => "نقداً",
                PaymentMethod.Visa => "فيزا كارد",
                PaymentMethod.MasterCard => "ماستركارد",
                PaymentMethod.InstaPay => "انستاباي",
                PaymentMethod.VodafoneCash => "فودافون كاش",
                PaymentMethod.OrangeMoney => "أورانج كاش",
                PaymentMethod.EtisalatCash => "اتصالات كاش",
                PaymentMethod.LoyaltyPoints => "نقاط الولاء",
                PaymentMethod.Voucher => "قسيمة شراء",
                PaymentMethod.Split => "دفع متعدد",
                _ => m.ToString()
            } : m switch
            {
                PaymentMethod.Cash => "Cash",
                PaymentMethod.Visa => "Visa Card",
                PaymentMethod.MasterCard => "MasterCard",
                PaymentMethod.InstaPay => "InstaPay",
                PaymentMethod.VodafoneCash => "Vodafone Cash",
                PaymentMethod.OrangeMoney => "Orange Cash",
                PaymentMethod.EtisalatCash => "Etisalat Cash",
                PaymentMethod.LoyaltyPoints => "Loyalty Points",
                PaymentMethod.Voucher => "Voucher",
                PaymentMethod.Split => "Split Payment",
                _ => m.ToString()
            }
        });

        return Ok(APIBaseResponse<object>.Success(methods));
    }

    /// <summary>
    /// Get invoice types
    /// </summary>
    [HttpGet("invoice-types")]
    [ProducesResponseType(typeof(APIBaseResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetInvoiceTypes()
    {
        var isArabic = LocalizationHelper.IsArabic;
        var types = Enum.GetValues<InvoiceType>().Select(t => new
        {
            Id = (int)t,
            Name = t.ToString(),
            DisplayName = isArabic ? t switch
            {
                InvoiceType.Sale => "فاتورة بيع",
                InvoiceType.Return => "مرتجع",
                InvoiceType.Quotation => "عرض أسعار",
                _ => t.ToString()
            } : t.ToString()
        });

        return Ok(APIBaseResponse<object>.Success(types));
    }

    /// <summary>
    /// Get shift statuses
    /// </summary>
    [HttpGet("shift-statuses")]
    [ProducesResponseType(typeof(APIBaseResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetShiftStatuses()
    {
        var isArabic = LocalizationHelper.IsArabic;
        var statuses = Enum.GetValues<ShiftStatus>().Select(s => new
        {
            Id = (int)s,
            Name = s.ToString(),
            DisplayName = isArabic ? s switch
            {
                ShiftStatus.Open => "مفتوحة",
                ShiftStatus.Closed => "مغلقة",
                _ => s.ToString()
            } : s.ToString()
        });

        return Ok(APIBaseResponse<object>.Success(statuses));
    }

    /// <summary>
    /// Get stock movement types
    /// </summary>
    [HttpGet("stock-movement-types")]
    [ProducesResponseType(typeof(APIBaseResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetStockMovementTypes()
    {
        var isArabic = LocalizationHelper.IsArabic;
        var types = Enum.GetValues<StockMovementType>().Select(t => new
        {
            Id = (int)t,
            Name = t.ToString(),
            DisplayName = isArabic ? t switch
            {
                StockMovementType.InitialSeed => "رصيد افتتاحي",
                StockMovementType.Sale => "مبيعات",
                StockMovementType.Return => "مرتجع مبيعات",
                StockMovementType.Adjustment => "تسوية جردية",
                StockMovementType.TransferIn => "تحويل وارد",
                StockMovementType.TransferOut => "تحويل صادر",
                StockMovementType.Damaged => "تالف / هالك",
                _ => t.ToString()
            } : t.ToString()
        });

        return Ok(APIBaseResponse<object>.Success(types));
    }

    /// <summary>
    /// Get inventories lookup
    /// </summary>
    [HttpGet("inventories")]
    [ProducesResponseType(typeof(APIBaseResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventories([FromQuery] int? branchId)
    {
        var inventories = await _unitOfWork.Inventories.GetAllAsync(false, branchId);
        var dtos = inventories.Select(i => new
        {
            Id = i.Id,
            Name = LocalizationHelper.Localize(i.NameEn, i.NameAr),
            NameEn = i.NameEn,
            NameAr = i.NameAr,
            Code = i.Code,
            BranchId = i.BranchId,
            IsDefault = i.IsDefault
        });

        return Ok(APIBaseResponse<object>.Success(dtos));
    }
}
