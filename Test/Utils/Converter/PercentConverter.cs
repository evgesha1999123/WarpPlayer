using System;

namespace Test.Utils.Converter
{
    public class PercentConverter
    {
        public static string AsPercentToUI(double value, double min, double max) => $"{Math.Round((Math.Clamp(value, min, max) - min) * 100)} %";
    }
}
