using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;
using POS_Private.Domain.Entities;
using POS_Private.Domain.Interfaces;
using MediatR;

namespace POS_Private.Application.Queries.Customer;

public record SearchCustomersQuery(CustomerSearchFilterDto Filter) : IRequest<APIBaseResponse<PaginatedResultDto<CustomerDto>>>;

public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, APIBaseResponse<PaginatedResultDto<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchCustomersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<PaginatedResultDto<CustomerDto>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Customers.SearchAsync(
            request.Filter.SearchTerm,
            request.Filter.PageNumber,
            request.Filter.PageSize);

        var dtos = items.Select(c => new CustomerDto
        {
            Id = c.Id,
            Code = c.Code,
            FullName = c.FullName,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Address = c.Address,
            City = c.City,
            LoyaltyPoints = c.LoyaltyPoints,
            TotalSpent = c.TotalSpent,
            Notes = c.Notes,
            IsActive = c.IsActive,
            InvoicesCount = c.Invoices?.Count ?? 0
        }).ToList();

        var result = new PaginatedResultDto<CustomerDto>(dtos, totalCount, request.Filter.PageNumber, request.Filter.PageSize);
        return APIBaseResponse<PaginatedResultDto<CustomerDto>>.Success(result);
    }
}

public record GetCustomerByIdQuery(int Id) : IRequest<APIBaseResponse<CustomerDto>>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, APIBaseResponse<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<APIBaseResponse<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
        if (customer == null)
            throw new NotFoundException(nameof(Domain.Entities.Customer), request.Id);

        var dto = new CustomerDto
        {
            Id = customer.Id,
            Code = customer.Code,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address,
            City = customer.City,
            LoyaltyPoints = customer.LoyaltyPoints,
            TotalSpent = customer.TotalSpent,
            Notes = customer.Notes,
            IsActive = customer.IsActive,
            InvoicesCount = customer.Invoices?.Count ?? 0
        };

        return APIBaseResponse<CustomerDto>.Success(dto);
    }
}
