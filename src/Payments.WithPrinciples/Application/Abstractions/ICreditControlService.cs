using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

public interface ICreditControlService
{
    CreditLimitReport CheckLimits(Administrator administrator);

    OperationResult BlockCardForDebt(Administrator administrator, CreditCard card);
}
