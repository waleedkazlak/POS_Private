using POS_Private.Application.Commands.Customer;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace POS_Private.Tests.Customers;

public class CustomerCommandsTests
{
    [Fact]
    public async Task CreateCustomer_ValidCustomer_ShouldPersistWithGeneratedCode()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new CreateCustomerCommandHandler(uow);

        var dto = new CreateCustomerDto
        {
            FullName = "Ahmed Taha",
            PhoneNumber = "+201012345678",
            Email = "ahmed.taha@example.com",
            City = "Cairo",
            Address = "Nasr City, Cairo"
        };

        // Act
        var result = await handler.Handle(new CreateCustomerCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        int customerId = result.Data;
        customerId.Should().BeGreaterThan(0);

        var customer = await context.Customers.FirstAsync(c => c.Id == customerId);
        customer.FullName.Should().Be("Ahmed Taha");
        customer.PhoneNumber.Should().Be("+201012345678");
        customer.Code.Should().StartWith("CUST-");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_DuplicatePhoneNumber_ShouldThrowBadRequest()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new CreateCustomerCommandHandler(uow);

        var existing = await context.Customers.FirstAsync();

        var duplicateDto = new CreateCustomerDto
        {
            FullName = "Another Person",
            PhoneNumber = existing.PhoneNumber,
            City = "Giza"
        };

        // Act & Assert
        var act = async () => await handler.Handle(new CreateCustomerCommand(duplicateDto), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"*'{existing.PhoneNumber}' already exists*");
    }

    [Fact]
    public async Task UpdateCustomer_ShouldUpdatePropertiesCorrectly()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new UpdateCustomerCommandHandler(uow);

        var customer = await context.Customers.FirstAsync();

        var updateDto = new UpdateCustomerDto
        {
            Id = customer.Id,
            FullName = "Updated Customer Name",
            PhoneNumber = "+201099887766",
            Email = "updated@example.com",
            City = "Alexandria",
            Address = "Stanley, Alexandria",
            IsActive = true
        };

        // Act
        var result = await handler.Handle(new UpdateCustomerCommand(updateDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await context.Customers.FirstAsync(c => c.Id == customer.Id);
        updated.FullName.Should().Be("Updated Customer Name");
        updated.PhoneNumber.Should().Be("+201099887766");
        updated.City.Should().Be("Alexandria");
    }
}
