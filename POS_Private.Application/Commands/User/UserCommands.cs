using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.User;

public record CreateUserCommand(CreateUserDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Users.GetByUsernameAsync(request.Dto.Username.Trim());
        if (existing != null)
            throw new BadRequestException($"Username '{request.Dto.Username}' already exists.");

        var role = await _unitOfWork.Roles.GetByIdAsync(request.Dto.RoleId);
        if (role == null)
            throw new NotFoundException(nameof(Role), request.Dto.RoleId);

        // Standard BCrypt hashing
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Dto.Password);

        var lang = string.Equals(request.Dto.PreferredLanguage, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

        var user = new Domain.Entities.User
        {
            Username = request.Dto.Username.Trim(),
            Email = request.Dto.Email.Trim(),
            PasswordHash = passwordHash,
            FullName = request.Dto.FullName,
            PhoneNumber = request.Dto.PhoneNumber,
            RoleId = request.Dto.RoleId,
            BranchId = request.Dto.BranchId,
            PreferredLanguage = lang,
            IsActive = true
        };

        var id = await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "User created successfully");
    }
}

public record UpdateUserCommand(UpdateUserDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Dto.Id);
        if (user == null)
            throw new NotFoundException(nameof(Domain.Entities.User), request.Dto.Id);

        var lang = string.Equals(request.Dto.PreferredLanguage, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

        user.FullName = request.Dto.FullName;
        user.PhoneNumber = request.Dto.PhoneNumber;
        user.RoleId = request.Dto.RoleId;
        user.BranchId = request.Dto.BranchId;
        user.PreferredLanguage = lang;
        user.IsActive = request.Dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "User updated successfully");
    }
}
