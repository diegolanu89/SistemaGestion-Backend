using System.Globalization;

namespace bdt_evm_app.Helpers;

public static class MonthHelper
{
    public static string GetMonthLabel(string monthKey)
    {
        try
        {
            var parts = monthKey.Split('-');
            if (parts.Length != 2) return monthKey;
            var date = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1);
            var culture = new CultureInfo("es-AR");
            var name = date.ToString("MMMM", culture);
            return char.ToUpper(name[0]) + name[1..] + " " + date.Year;
        }
        catch { return monthKey; }
    }
}
