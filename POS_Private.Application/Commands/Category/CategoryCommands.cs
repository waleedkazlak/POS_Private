using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Category;

public record CreateCategoryCommand(CreateCategoryDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = !string.IsNullOrWhiteSpace(request.Dto.Slug) 
            ? request.Dto.Slug.Trim().ToLowerInvariant().Replace(" ", "-") 
            : request.Dto.NameEn.Trim().ToLowerInvariant().Replace(" ", "-");

        var category = new Domain.Entities.Category
        {
            NameEn = request.Dto.NameEn,
            NameAr = request.Dto.NameAr,
            Slug = slug,
            DescriptionEn = request.Dto.DescriptionEn,
            DescriptionAr = request.Dto.DescriptionAr,
            PictureUrl = request.Dto.PictureUrl,
            ParentCategoryId = request.Dto.ParentCategoryId,
            DisplayOrder = request.Dto.DisplayOrder,
            IsActive = request.Dto.IsActive
        };

        var id = await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Category created successfully");
    }
}

public record UpdateCategoryCommand(UpdateCategoryDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Dto.Id);
        if (category == null)
            throw new NotFoundException(nameof(Category), request.Dto.Id);

        var slug = !string.IsNullOrWhiteSpace(request.Dto.Slug)
            ? request.Dto.Slug.Trim().ToLowerInvariant().Replace(" ", "-")
            : request.Dto.NameEn.Trim().ToLowerInvariant().Replace(" ", "-");

        category.NameEn = request.Dto.NameEn;
        category.NameAr = request.Dto.NameAr;
        category.Slug = slug;
        category.DescriptionEn = request.Dto.DescriptionEn;
        category.DescriptionAr = request.Dto.DescriptionAr;
        category.PictureUrl = request.Dto.PictureUrl;
        category.ParentCategoryId = request.Dto.ParentCategoryId;
        category.DisplayOrder = request.Dto.DisplayOrder;
        category.IsActive = request.Dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Category updated successfully");
    }
}

public record DeleteCategoryCommand(int Id) : IRequest<APIBaseResponse<bool>>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);
        if (category == null)
            throw new NotFoundException(nameof(Category), request.Id);

        await _unitOfWork.Categories.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Category deleted successfully");
    }
}
