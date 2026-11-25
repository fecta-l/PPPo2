using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

public interface IAccountService
{
    OperationResult Cancel(Client client, Account account);
}
