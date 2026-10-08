using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Product;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Dto.CategoryId);
        if (category == null)
            throw new NotFoundException(nameof(Domain.Entities.Category), request.Dto.CategoryId);

        var slug = !string.IsNullOrWhiteSpace(request.Dto.Slug)
            ? request.Dto.Slug.Trim().ToLowerInvariant().Replace(" ", "-")
            : request.Dto.NameEn.Trim().ToLowerInvariant().Replace(" ", "-");

        var product = new Domain.Entities.Product
        {
            CategoryId = request.Dto.CategoryId,
            NameEn = request.Dto.NameEn,
            NameAr = request.Dto.NameAr,
            Slug = slug,
            DescriptionEn = request.Dto.DescriptionEn,
            DescriptionAr = request.Dto.DescriptionAr,
            SKU = request.Dto.SKU,
            Barcode = request.Dto.Barcode,
            BasePrice = request.Dto.BasePrice,
            CostPrice = request.Dto.CostPrice,
            CompareAtPrice = request.Dto.CompareAtPrice,
            PictureUrl = request.Dto.PictureUrl,
            Tags = request.Dto.Tags,
            TrackStock = request.Dto.TrackStock,
            IsActive = request.Dto.IsActive,
            IsFeatured = request.Dto.IsFeatured
        };

        if (request.Dto.Variants != null && request.Dto.Variants.Any())
        {
            foreach (var v in request.Dto.Variants)
            {
                product.Variants.Add(new ProductVariant
                {
                    ColorId = v.ColorId,
                    SizeId = v.SizeId,
                    SKU = v.SKU,
                    Barcode = v.Barcode,
                    PriceOverride = v.PriceOverride,
                    CostPriceOverride = v.CostPriceOverride,
                    ImageUrl = v.ImageUrl,
                    StockQuantity = v.InitialStock,
                    IsActive = true
                });
            }
        }

        var id = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Product created successfully");
    }
}

public record UpdateProductCommand(UpdateProductDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(request.Dto.Id);
        if (product == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), request.Dto.Id);

        var slug = !string.IsNullOrWhiteSpace(request.Dto.Slug)
            ? request.Dto.Slug.Trim().ToLowerInvariant().Replace(" ", "-")
            : request.Dto.NameEn.Trim().ToLowerInvariant().Replace(" ", "-");

        product.CategoryId = request.Dto.CategoryId;
        product.NameEn = request.Dto.NameEn;
        product.NameAr = request.Dto.NameAr;
        product.Slug = slug;
        product.DescriptionEn = request.Dto.DescriptionEn;
        product.DescriptionAr = request.Dto.DescriptionAr;
        product.SKU = request.Dto.SKU;
        product.Barcode = request.Dto.Barcode;
        product.BasePrice = request.Dto.BasePrice;
        product.CostPrice = request.Dto.CostPrice;
        product.CompareAtPrice = request.Dto.CompareAtPrice;
        product.PictureUrl = request.Dto.PictureUrl;
        product.Tags = request.Dto.Tags;
        product.TrackStock = request.Dto.TrackStock;
        product.IsActive = request.Dto.IsActive;
        product.IsFeatured = request.Dto.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;

        if (request.Dto.Variants != null)
        {
            var incomingVariants = request.Dto.Variants;

            // Remove variants that are not present in incoming list
            var variantsToRemove = product.Variants
                .Where(ev => !incomingVariants.Any(iv => 
                    (iv.Id.HasValue && iv.Id.Value > 0 && iv.Id.Value == ev.Id) ||
                    (!iv.Id.HasValue && iv.ColorId == ev.ColorId && iv.SizeId == ev.SizeId && (!string.IsNullOrEmpty(iv.SKU) && iv.SKU == ev.SKU))
                ))
                .ToList();

            foreach (var toRemove in variantsToRemove)
            {
                product.Variants.Remove(toRemove);
            }

            foreach (var incoming in incomingVariants)
            {
                ProductVariant? existing = null;
                if (incoming.Id.HasValue && incoming.Id.Value > 0)
                {
                    existing = product.Variants.FirstOrDefault(v => v.Id == incoming.Id.Value);
                }
                else
                {
                    existing = product.Variants.FirstOrDefault(v => 
                        (incoming.ColorId.HasValue && v.ColorId == incoming.ColorId && incoming.SizeId.HasValue && v.SizeId == incoming.SizeId) ||
                        (!string.IsNullOrEmpty(incoming.SKU) && v.SKU == incoming.SKU)
                    );
                }

                if (existing != null)
                {
                    existing.ColorId = incoming.ColorId;
                    existing.SizeId = incoming.SizeId;
                    existing.SKU = incoming.SKU;
                    existing.Barcode = incoming.Barcode;
                    existing.PriceOverride = incoming.PriceOverride;
                    existing.CostPriceOverride = incoming.CostPriceOverride;
                    existing.ImageUrl = incoming.ImageUrl;
                    if (incoming.InitialStock > 0 || existing.StockQuantity == 0)
                    {
                        existing.StockQuantity = incoming.InitialStock;
                    }
                    existing.IsActive = true;
                }
                else
                {
                    product.Variants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        ColorId = incoming.ColorId,
                        SizeId = incoming.SizeId,
                        SKU = incoming.SKU,
                        Barcode = incoming.Barcode,
                        PriceOverride = incoming.PriceOverride,
                        CostPriceOverride = incoming.CostPriceOverride,
                        ImageUrl = incoming.ImageUrl,
                        StockQuantity = incoming.InitialStock,
                        IsActive = true
                    });
                }
            }
        }

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Product updated successfully");
    }
}

public record DeleteProductCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        await _unitOfWork.Products.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Product deleted successfully");
    }
}
