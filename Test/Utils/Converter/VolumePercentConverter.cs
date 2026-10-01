using Microsoft.UI.Xaml.Data;
using System;


namespace Test.Utils.Converter
{
    public class VolumePercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double volume)
            {
                return PercentConverter.AsPercentToUI(volume, 0, 1);
            }

            return "0 %";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
