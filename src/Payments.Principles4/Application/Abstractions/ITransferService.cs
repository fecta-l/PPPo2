using Payments.Principles4.Application.Models;
using Payments.Principles4.Domain;

namespace Payments.Principles4.Application.Abstractions;

public interface ITransferService
{
    TransferResult Transfer(Client client, Account from, Account to, decimal amount);
}
