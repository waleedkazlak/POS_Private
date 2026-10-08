using POS_Private.Application.DTOs;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Coupon;

public record ValidateCouponQuery(ValidateCouponDto Dto) : IRequest<APIBaseResponse<CouponValidationResultDto>>;

public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, APIBaseResponse<CouponValidationResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ValidateCouponQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<CouponValidationResultDto>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var cleanCode = request.Dto.Code.Trim().ToUpperInvariant();
        var coupon = await _unitOfWork.Coupons.GetByCodeAsync(cleanCode);

        if (coupon == null || !coupon.IsActive)
        {
            return APIBaseResponse<CouponValidationResultDto>.Success(new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Invalid or inactive coupon code."
            });
        }

        var now = DateTime.UtcNow;
        if (coupon.StartDate.HasValue && coupon.StartDate.Value > now)
        {
            return APIBaseResponse<CouponValidationResultDto>.Success(new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Coupon is not active yet."
            });
        }

        if (coupon.EndDate.HasValue && coupon.EndDate.Value < now)
        {
            return APIBaseResponse<CouponValidationResultDto>.Success(new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Coupon has expired."
            });
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            return APIBaseResponse<CouponValidationResultDto>.Success(new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Coupon usage limit has been reached."
            });
        }

        if (coupon.MinOrderAmount.HasValue && request.Dto.OrderSubTotal < coupon.MinOrderAmount.Value)
        {
            return APIBaseResponse<CouponValidationResultDto>.Success(new CouponValidationResultDto
            {
                IsValid = false,
                Message = $"Minimum order amount of {coupon.MinOrderAmount.Value:N2} EGP required."
            });
        }

        decimal discountAmount = 0;
        if (coupon.DiscountType == DiscountType.Percentage)
        {
            discountAmount = request.Dto.OrderSubTotal * (coupon.DiscountValue / 100m);
            if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                discountAmount = coupon.MaxDiscountAmount.Value;
        }
        else
        {
            discountAmount = coupon.DiscountValue;
        }

        return APIBaseResponse<CouponValidationResultDto>.Success(new CouponValidationResultDto
        {
            IsValid = true,
            CouponId = coupon.Id,
            Code = coupon.Code,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            CalculatedDiscountAmount = discountAmount,
            Message = "Coupon applied successfully"
        });
    }
}

public record GetCouponsQuery() : IRequest<APIBaseResponse<IEnumerable<CouponDto>>>;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, APIBaseResponse<IEnumerable<CouponDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCouponsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<CouponDto>>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var coupons = await _unitOfWork.Coupons.GetAllAsync();
        var dtos = coupons.Select(c => new CouponDto
        {
            Id = c.Id,
            Code = c.Code,
            Description = c.Description,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            MinOrderAmount = c.MinOrderAmount,
            MaxDiscountAmount = c.MaxDiscountAmount,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            UsageLimit = c.UsageLimit,
            UsageCount = c.UsageCount,
            IsActive = c.IsActive
        }).ToList();

        return APIBaseResponse<IEnumerable<CouponDto>>.Success(dtos);
    }
}
