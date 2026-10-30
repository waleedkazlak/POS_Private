using POS_Private.Application.Commands.Category;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Tests.Catalog;

public class CategoryAndProductTests
{
    [Fact]
    public async Task CreateCategory_ShouldGenerateSlugAndPersist()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new CreateCategoryCommandHandler(uow);

        var dto = new CreateCategoryDto
        {
            NameEn = "Urban Outerwear",
            NameAr = "ملابس خارجية عصرية",
            DescriptionEn = "Jackets, trench coats and windbreakers",
            IsActive = true,
            DisplayOrder = 1
        };

        // Act
        var result = await handler.Handle(new CreateCategoryCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        int categoryId = result.Data;

        var category = await context.Categories.FirstAsync(c => c.Id == categoryId);
        category.NameEn.Should().Be("Urban Outerwear");
        category.Slug.Should().Be("urban-outerwear");
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCategory_ShouldModifyFieldsAndSetUpdatedAt()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new UpdateCategoryCommandHandler(uow);

        var existingCategory = await context.Categories.FirstAsync();

        var updateDto = new UpdateCategoryDto
        {
            Id = existingCategory.Id,
            NameEn = "Updated Name EN",
            NameAr = "اسم محدث",
            Slug = "updated-custom-slug",
            IsActive = false
        };

        // Act
        var result = await handler.Handle(new UpdateCategoryCommand(updateDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await context.Categories.FirstAsync(c => c.Id == existingCategory.Id);
        updated.NameEn.Should().Be("Updated Name EN");
        updated.Slug.Should().Be("updated-custom-slug");
        updated.IsActive.Should().BeFalse();
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteCategory_NonExistent_ShouldThrowNotFoundException()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new DeleteCategoryCommandHandler(uow);

        // Act & Assert
        var act = async () => await handler.Handle(new DeleteCategoryCommand(999999), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public void ProductVariant_EffectivePrice_ShouldFallbackToBasePriceOrUsePriceOverride()
    {
        // Arrange & Act
        var product = new Product
        {
            NameEn = "Heavy Oversized Hoodie",
            BasePrice = 750.00m
        };

        var variantStandard = new ProductVariant
        {
            Product = product,
            PriceOverride = null
        };

        var variantSpecialCut = new ProductVariant
        {
            Product = product,
            PriceOverride = 870.00m
        };

        // Assert
        variantStandard.EffectivePrice.Should().Be(750.00m);
        variantSpecialCut.EffectivePrice.Should().Be(870.00m);
    }
}
