using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Commands.Customer;

public record CreateCustomerCommand(CreateCustomerDto Dto) : IRequest<APIBaseResponse<int>>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, APIBaseResponse<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<int>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var cleanPhone = request.Dto.PhoneNumber.Trim();
        var existing = await _unitOfWork.Customers.GetByPhoneNumberAsync(cleanPhone);
        if (existing != null)
            throw new BadRequestException($"Customer with phone number '{cleanPhone}' already exists.");

        var customer = new Domain.Entities.Customer
        {
            Code = $"CUST-{DateTime.UtcNow:yyyyMMddHHmmss}",
            FullName = request.Dto.FullName,
            PhoneNumber = cleanPhone,
            Email = request.Dto.Email,
            Address = request.Dto.Address,
            City = request.Dto.City,
            Notes = request.Dto.Notes,
            IsActive = true
        };

        var id = await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<int>.Success(id, "Customer created successfully");
    }
}

public record UpdateCustomerCommand(UpdateCustomerDto Dto) : IRequest<APIBaseResponse<bool>>;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, APIBaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<bool>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Dto.Id);
        if (customer == null)
            throw new NotFoundException(nameof(Domain.Entities.Customer), request.Dto.Id);

        customer.FullName = request.Dto.FullName;
        customer.PhoneNumber = request.Dto.PhoneNumber.Trim();
        customer.Email = request.Dto.Email;
        customer.Address = request.Dto.Address;
        customer.City = request.Dto.City;
        customer.Notes = request.Dto.Notes;
        customer.IsActive = request.Dto.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return APIBaseResponse<bool>.Success(true, "Customer updated successfully");
    }
}
