using System.Globalization;

namespace POS_Private.Application.Common;

public static class LocalizationHelper
{
    public static bool IsArabic =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);

    public static string Localize(string? en, string? ar)
    {
        if (IsArabic && !string.IsNullOrWhiteSpace(ar))
            return ar;
        return en ?? string.Empty;
    }

    public static string? LocalizeNullable(string? en, string? ar)
    {
        if (IsArabic && !string.IsNullOrWhiteSpace(ar))
            return ar;
        return !string.IsNullOrWhiteSpace(en) ? en : null;
    }
}
