

using Microsoft.UI.Xaml.Data;
using System;

namespace Test.Utils.Converter
{
    public class SpeedPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double speed)
            {
                return PercentConverter.AsPercentToUI(speed, 0, 2);
            }

            return "0 %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
