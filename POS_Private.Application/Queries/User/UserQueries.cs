using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.User;

public record GetUsersQuery() : IRequest<APIBaseResponse<IEnumerable<UserDto>>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, APIBaseResponse<IEnumerable<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var dtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            PhoneNumber = u.PhoneNumber,
            RoleId = u.RoleId,
            RoleName = u.Role?.Name ?? string.Empty,
            BranchId = u.BranchId,
            BranchName = LocalizationHelper.LocalizeNullable(u.Branch?.NameEn, u.Branch?.NameAr),
            PreferredLanguage = u.PreferredLanguage,
            IsActive = u.IsActive
        }).ToList();

        return APIBaseResponse<IEnumerable<UserDto>>.Success(dtos);
    }
}

public record GetUserByIdQuery(int Id) : IRequest<APIBaseResponse<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, APIBaseResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null)
            throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        var dto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            BranchId = user.BranchId,
            BranchName = LocalizationHelper.LocalizeNullable(user.Branch?.NameEn, user.Branch?.NameAr),
            PreferredLanguage = user.PreferredLanguage,
            IsActive = user.IsActive
        };

        return APIBaseResponse<UserDto>.Success(dto);
    }
}

public record GetRolesQuery() : IRequest<APIBaseResponse<IEnumerable<RoleDto>>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, APIBaseResponse<IEnumerable<RoleDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        var dtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Permissions = r.RolePermissions.Select(rp => new RolePermissionDto
            {
                ScreenId = rp.ScreenId,
                ScreenName = rp.Screen?.Name ?? string.Empty,
                ScreenCode = rp.Screen?.Code ?? string.Empty,
                CanView = rp.CanView,
                CanAdd = rp.CanAdd,
                CanEdit = rp.CanEdit,
                CanDelete = rp.CanDelete
            }).ToList()
        }).ToList();

        return APIBaseResponse<IEnumerable<RoleDto>>.Success(dtos);
    }
}
