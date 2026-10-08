using POS_Private.Application.Common;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Branch;

public record GetBranchesQuery() : IRequest<APIBaseResponse<IEnumerable<BranchDto>>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, APIBaseResponse<IEnumerable<BranchDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBranchesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<BranchDto>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _unitOfWork.Branches.GetAllAsync();
        var dtos = branches.Select(b =>
        {
            var branchName = LocalizationHelper.Localize(b.NameEn, b.NameAr);
            return new BranchDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = branchName,
                Address = b.Address,
                City = b.City,
                Phone = b.Phone,
                Email = b.Email,
                TaxNumber = b.TaxNumber,
                CommercialRegistration = b.CommercialRegistration,
                IsActive = b.IsActive,
                Registers = b.Registers.Select(r => new PosRegisterDto
                {
                    Id = r.Id,
                    RegisterCode = r.RegisterCode,
                    Name = r.Name,
                    BranchId = r.BranchId,
                    BranchName = branchName,
                    IsActive = r.IsActive
                }).ToList()
            };
        }).ToList();

        return APIBaseResponse<IEnumerable<BranchDto>>.Success(dtos);
    }
}

public record GetPosRegistersQuery(int? BranchId) : IRequest<APIBaseResponse<IEnumerable<PosRegisterDto>>>;

public class GetPosRegistersQueryHandler : IRequestHandler<GetPosRegistersQuery, APIBaseResponse<IEnumerable<PosRegisterDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPosRegistersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<IEnumerable<PosRegisterDto>>> Handle(GetPosRegistersQuery request, CancellationToken cancellationToken)
    {
        var registers = request.BranchId.HasValue
            ? await _unitOfWork.PosRegisters.GetByBranchIdAsync(request.BranchId.Value)
            : await _unitOfWork.PosRegisters.GetAllAsync();

        var dtos = registers.Select(r => new PosRegisterDto
        {
            Id = r.Id,
            RegisterCode = r.RegisterCode,
            Name = r.Name,
            BranchId = r.BranchId,
            BranchName = LocalizationHelper.Localize(r.Branch?.NameEn, r.Branch?.NameAr),
            IsActive = r.IsActive
        }).ToList();

        return APIBaseResponse<IEnumerable<PosRegisterDto>>.Success(dtos);
    }
}
