using Microsoft.UI.Xaml.Data;
using System;


namespace Test.Utils.Converter
{
    public partial class PlaybackTimeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double seconds)
            {
                return TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
            }

            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string ToUI_MM_SS(TimeSpan value)
        {
            var seconds = value.TotalSeconds;
            return TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
        }
    }
}
