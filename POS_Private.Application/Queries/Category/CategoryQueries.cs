using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Category;

public record GetCategoriesQuery() : IRequest<APIBaseResponse<IEnumerable<CategoryDto>>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, APIBaseResponse<IEnumerable<CategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = LocalizationHelper.Localize(c.NameEn, c.NameAr),
            NameEn = c.NameEn,
            NameAr = c.NameAr,
            Slug = c.Slug,
            Description = LocalizationHelper.LocalizeNullable(c.DescriptionEn, c.DescriptionAr),
            DescriptionEn = c.DescriptionEn,
            DescriptionAr = c.DescriptionAr,
            PictureUrl = c.PictureUrl,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = LocalizationHelper.LocalizeNullable(c.ParentCategory?.NameEn, c.ParentCategory?.NameAr),
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ProductsCount = c.Products?.Count ?? 0
        }).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);

        return APIBaseResponse<IEnumerable<CategoryDto>>.Success(dtos);
    }
}

public record GetCategoryByIdQuery(int Id) : IRequest<APIBaseResponse<CategoryDto>>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, APIBaseResponse<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);
        if (category == null)
            throw new NotFoundException(nameof(Category), request.Id);

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = LocalizationHelper.Localize(category.NameEn, category.NameAr),
            NameEn = category.NameEn,
            NameAr = category.NameAr,
            Slug = category.Slug,
            Description = LocalizationHelper.LocalizeNullable(category.DescriptionEn, category.DescriptionAr),
            DescriptionEn = category.DescriptionEn,
            DescriptionAr = category.DescriptionAr,
            PictureUrl = category.PictureUrl,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = LocalizationHelper.LocalizeNullable(category.ParentCategory?.NameEn, category.ParentCategory?.NameAr),
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductsCount = category.Products?.Count ?? 0
        };

        return APIBaseResponse<CategoryDto>.Success(dto);
    }
}
