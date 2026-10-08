using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Branch;

public record CreateBranchCommand(CreateBranchDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Branches.GetByCodeAsync(request.Dto.Code);
        if (existing != null)
            throw new BadRequestException($"Branch code '{request.Dto.Code}' already exists.");

        var branch = new Domain.Entities.Branch
        {
            Code = request.Dto.Code.Trim().ToUpperInvariant(),
            NameEn = request.Dto.NameEn,
            NameAr = request.Dto.NameAr,
            Address = request.Dto.Address,
            City = request.Dto.City,
            Phone = request.Dto.Phone,
            Email = request.Dto.Email,
            TaxNumber = request.Dto.TaxNumber,
            CommercialRegistration = request.Dto.CommercialRegistration,
            IsActive = request.Dto.IsActive
        };

        var id = await _unitOfWork.Branches.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Branch created successfully");
    }
}

public record CreatePosRegisterCommand(CreatePosRegisterDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreatePosRegisterCommandHandler : IRequestHandler<CreatePosRegisterCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePosRegisterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreatePosRegisterCommand request, CancellationToken cancellationToken)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(request.Dto.BranchId);
        if (branch == null)
            throw new NotFoundException(nameof(Domain.Entities.Branch), request.Dto.BranchId);

        var register = new PosRegister
        {
            BranchId = request.Dto.BranchId,
            RegisterCode = request.Dto.RegisterCode.Trim().ToUpperInvariant(),
            Name = request.Dto.Name,
            IsActive = request.Dto.IsActive
        };

        var id = await _unitOfWork.PosRegisters.AddAsync(register);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "POS Register created successfully");
    }
}
