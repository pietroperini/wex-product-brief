using FluentValidation;

namespace ProductBrief.Models.Validators;

public class CreatePurchaseTransactionValidator : AbstractValidator<CreatePurchaseTransactionRequest>
{
    public CreatePurchaseTransactionValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(50).WithMessage("Description must not exceed 50 characters.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Transaction date cannot be in the future.");

        RuleFor(x => x.PurchaseAmount)
            .NotEmpty().WithMessage("Purchase amount is required.")
            .GreaterThan(0).WithMessage("Purchase amount must be a valid positive amount.")
            .Must(amount => Math.Round(amount, 2) == amount)
            .WithMessage("Purchase amount must be rounded to the nearest cent.");
    }
}
