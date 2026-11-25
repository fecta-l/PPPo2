# Практическая работа №4. Система «Платежи» (вариант 2)

Переработка консольного приложения из практической работы №3 с применением принципов
**LSP** (подстановка Лисков), **ISP** (разделение интерфейсов) и **DIP** (инверсия зависимостей).

## Состав

| Путь | Назначение |
| --- | --- |
| `Практика 4_Матийцива О.Р..docx` | Отчёт (12 страниц, 3 рисунка, 4 таблицы, листинги) |
| `Практика 4_Матийцива О.Р..pdf` | PDF-версия отчёта |
| `src/Payments.Principles4` | Приложение: домен, платёжные инструменты, сервисы, инфраструктура |
| `tests/Payments.Principles4.Tests` | 9 модульных тестов (LSP, ISP, DIP) |
| `docs/img` | UML-диаграмма и протоколы выполнения |
| `tools` | Скрипты генерации диаграммы, консольных изображений и отчёта |

## Запуск

Требуется .NET SDK 9.0.

```bash
dotnet run --project src/Payments.Principles4
dotnet run --project tests/Payments.Principles4.Tests
```

Тесты должны вывести `Пройдено тестов: 9 из 9.` и вернуть код 0.

## Что демонстрируется

- **LSP** — абстрактный `PaymentInstrument` и три взаимозаменяемых наследника
  (`CardInstrument`, `AccountInstrument`, `BonusInstrument`); `OrderPaymentService`
  работает с базовым типом и не содержит `switch`.
- **ISP** — узкие интерфейсы `IOrderPaymentService`, `ITransferService`, `ICardService`,
  `ICreditControlService`, `IPaymentNotifier`, `IOperationNotifier`, `IBlockable`,
  `ILimitInfo`; бонусный инструмент не реализует карточные возможности.
- **DIP** — сервисы зависят от `IPaymentJournal` и `IPaymentNotifier`, конкретные
  `ConsoleNotifier`, `InMemoryPaymentJournal` и репозитории создаются в `AppComposition`;
  тесты подменяют уведомления заглушками.
