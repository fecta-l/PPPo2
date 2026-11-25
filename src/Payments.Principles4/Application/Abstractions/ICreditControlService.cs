using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain;
using Payments.Principles4.Domain.Capabilities;

namespace Payments.Principles4.Application.Abstractions;

/// <summary>
/// Административные операции. Клиентский код не зависит от этого интерфейса (ISP).
/// </summary>
public interface ICreditControlService
{
    CreditLimitReport CheckLimits(Administrator administrator);

    OperationResult BlockCardForDebt(Administrator administrator, ICreditCard card);
}
