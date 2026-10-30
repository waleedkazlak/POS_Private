using POS_Private.Application.Commands.Auth;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace POS_Private.Tests.Auth;

public class AuthCommandsTests
{
    [Fact]
    public async Task UpdateProfilePicture_ValidUser_ShouldUpdateAndPersist()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new UpdateProfilePictureCommandHandler(uow);

        var user = await context.Users.FirstAsync();
        string newAvatar = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

        // Act
        var result = await handler.Handle(new UpdateProfilePictureCommand(user.Id, newAvatar), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(newAvatar);

        var updatedUser = await context.Users.FirstAsync(u => u.Id == user.Id);
        updatedUser.ProfilePictureUrl.Should().Be(newAvatar);
    }

    [Fact]
    public async Task ChangePassword_CorrectCurrentPassword_ShouldHashAndSaveNewPassword()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new ChangePasswordCommandHandler(uow);

        var user = await context.Users.FirstAsync();
        string currentPlain = "Admin@123";
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(currentPlain);
        await context.SaveChangesAsync();

        var dto = new ChangePasswordDto
        {
            CurrentPassword = currentPlain,
            NewPassword = "NewStrongPassword2026!",
            ConfirmNewPassword = "NewStrongPassword2026!"
        };

        // Act
        var result = await handler.Handle(new ChangePasswordCommand(user.Id, dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedUser = await context.Users.FirstAsync(u => u.Id == user.Id);
        BCrypt.Net.BCrypt.Verify("NewStrongPassword2026!", updatedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ShouldThrowBadRequest()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new ChangePasswordCommandHandler(uow);

        var user = await context.Users.FirstAsync();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword123!");
        await context.SaveChangesAsync();

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword999",
            NewPassword = "NewStrongPassword2026!",
            ConfirmNewPassword = "NewStrongPassword2026!"
        };

        // Act & Assert
        var act = async () => await handler.Handle(new ChangePasswordCommand(user.Id, dto), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*current password provided is incorrect*");
    }

    [Fact]
    public async Task ChangePassword_MismatchConfirmation_ShouldThrowBadRequest()
    {
        // Arrange
        var (context, uow) = await TestDatabaseFactory.CreateSeededContextAsync();
        var handler = new ChangePasswordCommandHandler(uow);

        var user = await context.Users.FirstAsync();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword123!");
        await context.SaveChangesAsync();

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "RealPassword123!",
            NewPassword = "NewStrongPassword2026!",
            ConfirmNewPassword = "DifferentPassword999!"
        };

        // Act & Assert
        var act = async () => await handler.Handle(new ChangePasswordCommand(user.Id, dto), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*do not match*");
    }
}
