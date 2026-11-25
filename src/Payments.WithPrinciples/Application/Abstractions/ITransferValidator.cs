using Payments.WithPrinciples.Application.Models;
using Payments.WithPrinciples.Domain;

namespace Payments.WithPrinciples.Application.Abstractions;

public interface ITransferValidator
{
    ValidationResult Validate(Client client, Account from, Account to, decimal amount);
}
