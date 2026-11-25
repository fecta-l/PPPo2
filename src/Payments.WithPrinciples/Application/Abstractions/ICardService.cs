using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

public interface ICardService
{
    OperationResult Block(Client client, CreditCard card, string reason);
}
