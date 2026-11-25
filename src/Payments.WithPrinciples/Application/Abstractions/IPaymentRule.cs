using Payments.WithPrinciples.Application.Models;

namespace Payments.WithPrinciples.Application.Abstractions;

/// <summary>
/// Правило проверки платежа. Точка расширения OCP: новое правило — новый класс,
/// подключаемый в композиционном корне.
/// </summary>
public interface IPaymentRule
{
    string Name { get; }

    ValidationResult Check(PaymentContext context);
}
