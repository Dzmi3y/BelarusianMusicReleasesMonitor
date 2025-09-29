using CronExpressionDescriptor;
using System;
using System.Globalization;
using System.Windows.Data;

namespace BMRM.Desktop.Utils;

public class CronToHumanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var cron = value as string;
        if (string.IsNullOrWhiteSpace(cron))
            return "—";

        try
        {
            var expression = ExpressionDescriptor.GetDescription(cron, new Options()
            {
                Use24HourTimeFormat = true,
                Locale = "en"
            });
            return expression;
        }
        catch
        {
            return "Invalid cron format";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
