using Payments.WithPrinciples.Application.Abstractions;
using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Common;

namespace Payments.WithPrinciples.Application.Payments.Rules;

/// <summary>
/// Новое правило, добавленное при расширении системы: ограничение суммы одного платежа.
/// Подключается в композиционном корне; OrderPaymentService и другие правила не изменяются.
/// </summary>
public sealed class DailyLimitRule : IPaymentRule
{
    private readonly decimal _limit;

    public DailyLimitRule(decimal limit)
    {
        if (limit <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                "Лимит должен быть положительным.");
        }

        _limit = limit;
    }

    public string Name => "Дневной лимит платежа";

    public ValidationResult Check(PaymentContext context)
    {
        return context.TotalAmount > _limit
            ? ValidationResult.Deny(
                $"сумма платежа {Money.Format(context.TotalAmount)} руб. " +
                $"превышает дневной лимит {Money.Format(_limit)} руб.")
            : ValidationResult.Allow();
    }
}
