using POS_Private.Application.Commands.Auth;
using POS_Private.Application.Commands.Branch;
using POS_Private.Application.Commands.Category;
using POS_Private.Application.Commands.Coupon;
using POS_Private.Application.Commands.Customer;
using POS_Private.Application.Commands.Invoice;
using POS_Private.Application.Commands.Product;
using POS_Private.Application.Commands.Shift;
using FluentValidation;

namespace POS_Private.Application.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Dto.NameEn).NotEmpty().WithMessage("Category English name is required.").MaximumLength(100);
        RuleFor(x => x.Dto.NameAr).NotEmpty().WithMessage("Category Arabic name is required.").MaximumLength(100);
        RuleFor(x => x.Dto.DescriptionEn).NotEmpty().WithMessage("Category English description is required.").MaximumLength(500);
        RuleFor(x => x.Dto.DescriptionAr).NotEmpty().WithMessage("Category Arabic description is required.").MaximumLength(500);
    }
}

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Dto.Id).GreaterThan(0).WithMessage("Valid Category ID is required.");
        RuleFor(x => x.Dto.NameEn).NotEmpty().WithMessage("Category English name is required.").MaximumLength(100);
        RuleFor(x => x.Dto.NameAr).NotEmpty().WithMessage("Category Arabic name is required.").MaximumLength(100);
        RuleFor(x => x.Dto.DescriptionEn).NotEmpty().WithMessage("Category English description is required.").MaximumLength(500);
        RuleFor(x => x.Dto.DescriptionAr).NotEmpty().WithMessage("Category Arabic description is required.").MaximumLength(500);
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Dto.CategoryId).GreaterThan(0).WithMessage("Valid Category is required.");
        RuleFor(x => x.Dto.NameEn).NotEmpty().WithMessage("Product English name is required.").MaximumLength(200);
        RuleFor(x => x.Dto.NameAr).NotEmpty().WithMessage("Product Arabic name is required.").MaximumLength(200);
        RuleFor(x => x.Dto.DescriptionEn).NotEmpty().WithMessage("Product English description is required.").MaximumLength(1000);
        RuleFor(x => x.Dto.DescriptionAr).NotEmpty().WithMessage("Product Arabic description is required.").MaximumLength(1000);
        RuleFor(x => x.Dto.BasePrice).GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");
        RuleForEach(x => x.Dto.Variants).ChildRules(v =>
        {
            v.RuleFor(x => x.PriceOverride).GreaterThanOrEqualTo(0).When(x => x.PriceOverride.HasValue).WithMessage("Variant price override cannot be negative.");
        });
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Dto.Id).GreaterThan(0).WithMessage("Valid Product ID is required.");
        RuleFor(x => x.Dto.CategoryId).GreaterThan(0).WithMessage("Valid Category is required.");
        RuleFor(x => x.Dto.NameEn).NotEmpty().WithMessage("Product English name is required.").MaximumLength(200);
        RuleFor(x => x.Dto.NameAr).NotEmpty().WithMessage("Product Arabic name is required.").MaximumLength(200);
        RuleFor(x => x.Dto.DescriptionEn).NotEmpty().WithMessage("Product English description is required.").MaximumLength(1000);
        RuleFor(x => x.Dto.DescriptionAr).NotEmpty().WithMessage("Product Arabic description is required.").MaximumLength(1000);
        RuleFor(x => x.Dto.BasePrice).GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");
        RuleForEach(x => x.Dto.Variants).ChildRules(v =>
        {
            v.RuleFor(x => x.PriceOverride).GreaterThanOrEqualTo(0).When(x => x.PriceOverride.HasValue).WithMessage("Variant price override cannot be negative.");
        });
    }
}

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().WithMessage("Branch code is required.").MaximumLength(20);
        RuleFor(x => x.Dto.NameEn).NotEmpty().WithMessage("Branch English name is required.").MaximumLength(100);
        RuleFor(x => x.Dto.NameAr).NotEmpty().WithMessage("Branch Arabic name is required.").MaximumLength(100);
    }
}

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Dto.BranchId).GreaterThan(0).WithMessage("Branch ID is required.");
        RuleFor(x => x.Dto.Lines).NotEmpty().WithMessage("Invoice must have at least one product line.");
        RuleFor(x => x.Dto.Payments).NotEmpty().WithMessage("At least one payment must be provided.");
        RuleForEach(x => x.Dto.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).GreaterThan(0).WithMessage("Product ID must be greater than 0.");
            line.RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1.");
        });
    }
}

public class OpenShiftCommandValidator : AbstractValidator<OpenShiftCommand>
{
    public OpenShiftCommandValidator()
    {
        RuleFor(x => x.Dto.RegisterId).GreaterThan(0).WithMessage("Register ID is required.");
        RuleFor(x => x.Dto.StartingCash).GreaterThanOrEqualTo(0).WithMessage("Starting cash drawer float cannot be negative.");
    }
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Dto.FullName).NotEmpty().WithMessage("Customer name is required.");
        RuleFor(x => x.Dto.PhoneNumber).NotEmpty().WithMessage("Phone number is required.");
    }
}

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().WithMessage("Coupon code is required.");
        RuleFor(x => x.Dto.DiscountValue).GreaterThan(0).WithMessage("Discount value must be greater than 0.");
    }
}
