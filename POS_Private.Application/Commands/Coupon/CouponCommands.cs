using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Coupon;

public record CreateCouponCommand(CreateCouponDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCouponCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var cleanCode = request.Dto.Code.Trim().ToUpperInvariant();
        var existing = await _unitOfWork.Coupons.GetByCodeAsync(cleanCode);
        if (existing != null)
            throw new BadRequestException($"Coupon code '{cleanCode}' already exists.");

        var coupon = new Domain.Entities.Coupon
        {
            Code = cleanCode,
            Description = request.Dto.Description,
            DiscountType = request.Dto.DiscountType,
            DiscountValue = request.Dto.DiscountValue,
            MinOrderAmount = request.Dto.MinOrderAmount,
            MaxDiscountAmount = request.Dto.MaxDiscountAmount,
            StartDate = request.Dto.StartDate,
            EndDate = request.Dto.EndDate,
            UsageLimit = request.Dto.UsageLimit,
            IsActive = request.Dto.IsActive
        };

        var id = await _unitOfWork.Coupons.AddAsync(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Coupon created successfully");
    }
}

public record DeleteCouponCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCouponCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(request.Id);
        if (coupon == null)
            throw new NotFoundException(nameof(Domain.Entities.Coupon), request.Id);

        await _unitOfWork.Coupons.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Coupon deleted successfully");
    }
}
