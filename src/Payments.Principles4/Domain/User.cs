namespace Payments.Principles4.Domain;

/// <summary>
/// Базовый класс пользователя системы. Отвечает только за идентификацию и имя.
/// </summary>
public abstract class User
{
    protected User(int id, string name)
    {
        if (id <= 0)
        {
            throw new DomainException("Идентификатор пользователя должен быть положительным.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Имя пользователя не может быть пустым.");
        }

        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string Name { get; }
}

