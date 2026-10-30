using POS_Private.Application.Commands.Inventory;
using POS_Private.Application.DTOs;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Enums;
using POS_Private.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace POS_Private.Tests.Inventory;

public class StockCommandsTests
{
    [Fact]
    public async Task AdjustStock_PositiveAdjustment_ShouldIncreaseStockAndLogMovement()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1, Username = "inventory.manager" };
        var handler = new AdjustStockCommandHandler(uow, currentUser);

        var product = await context.Products.Include(p => p.Variants).FirstAsync(p => p.Variants.Any());
        var variant = product.Variants.First();
        var inventory = await context.Inventories.FirstAsync();

        var initialStock = await context.BranchStocks
            .FirstOrDefaultAsync(s => s.InventoryId == inventory.Id && s.ProductId == product.Id && s.VariantId == variant.Id);
        int startingQty = initialStock?.QuantityOnHand ?? 0;

        int addedQty = 15;
        var dto = new AdjustStockDto
        {
            ProductId = product.Id,
            VariantId = variant.Id,
            InventoryId = inventory.Id,
            QuantityChange = addedQty,
            MovementType = StockMovementType.Adjustment,
            Notes = "Manual stock delivery count"
        };

        // Act
        var result = await handler.Handle(new AdjustStockCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.QuantityOnHand.Should().Be(startingQty + addedQty);

        var dbStock = await context.BranchStocks.FirstAsync(s => s.InventoryId == inventory.Id && s.ProductId == product.Id && s.VariantId == variant.Id);
        dbStock.QuantityOnHand.Should().Be(startingQty + addedQty);

        var movement = await context.StockMovements
            .Where(m => m.InventoryId == inventory.Id && m.ProductId == product.Id && m.VariantId == variant.Id)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        movement.Should().NotBeNull();
        movement!.QuantityChange.Should().Be(addedQty);
        movement.BalanceAfter.Should().Be(startingQty + addedQty);
        movement.MovementType.Should().Be(StockMovementType.Adjustment);
    }

    [Fact]
    public async Task AdjustStock_NegativeAdjustment_ShouldDecreaseStockCorrectly()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var currentUser = new MockCurrentUserService { UserId = 1, Username = "inventory.manager" };
        var handler = new AdjustStockCommandHandler(uow, currentUser);

        var product = await context.Products.Include(p => p.Variants).FirstAsync(p => p.Variants.Any());
        var variant = product.Variants.First();
        var inventory = await context.Inventories.FirstAsync();

        // Ensure stock is at least 30
        var stock = await context.BranchStocks.FirstOrDefaultAsync(s => s.InventoryId == inventory.Id && s.ProductId == product.Id && s.VariantId == variant.Id);
        if (stock == null)
        {
            stock = new BranchStock
            {
                InventoryId = inventory.Id,
                ProductId = product.Id,
                VariantId = variant.Id,
                QuantityOnHand = 30
            };
            await context.BranchStocks.AddAsync(stock);
        }
        else
        {
            stock.QuantityOnHand = 30;
        }
        await context.SaveChangesAsync();

        int reduction = -10;
        var dto = new AdjustStockDto
        {
            ProductId = product.Id,
            VariantId = variant.Id,
            InventoryId = inventory.Id,
            QuantityChange = reduction,
            MovementType = StockMovementType.Damaged,
            Notes = "Damaged merchandise write-off"
        };

        // Act
        var result = await handler.Handle(new AdjustStockCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.QuantityOnHand.Should().Be(20);

        var dbStock = await context.BranchStocks.FirstAsync(s => s.InventoryId == inventory.Id && s.ProductId == product.Id && s.VariantId == variant.Id);
        dbStock.QuantityOnHand.Should().Be(20);
    }
}
