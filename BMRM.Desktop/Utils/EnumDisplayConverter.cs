using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace BMRM.Desktop.Utils;

public class EnumDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var type = value.GetType();

        if (type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true)
        {
            var enumValue = value.ToString();
            var enumType = Nullable.GetUnderlyingType(type) ?? type;
            var field = enumType.GetField(enumValue);

            if (field != null)
            {
                var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description;
                if (!string.IsNullOrEmpty(description))
                    return description;

                var display = field.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()?.Name;
                if (!string.IsNullOrEmpty(display))
                    return display;
            }

            return enumValue;
        }

        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}