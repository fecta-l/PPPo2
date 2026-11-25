using System.Globalization;

namespace Payments.WithPrinciples.Common;

/// <summary>
/// Единое форматирование денежных сумм для консольного вывода.
/// </summary>
public static class Money
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public static string Format(decimal value) =>
        value.ToString("N2", RuCulture).Replace('\u00A0', ' ');
}
