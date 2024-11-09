using System.Text.RegularExpressions;

namespace TelerikCart.UITests.Core.Helpers;

public static class PriceUtils
{
    public static string NormalizePriceString(string price)
    {
        return Regex.Replace(price, @"[^0-9.]", "").Trim();
    }

    public static decimal ParsePrice(string price)
    {
        var normalizedPrice = NormalizePriceString(price);
        if (!decimal.TryParse(normalizedPrice, out var result))
        {
            throw new FormatException($"Unable to parse price: {price}");
        }
        return result;
    }
}