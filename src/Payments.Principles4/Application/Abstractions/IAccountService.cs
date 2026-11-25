using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Application.Abstractions;

public interface IAccountService
{
    OperationResult Cancel(Account account);
}
