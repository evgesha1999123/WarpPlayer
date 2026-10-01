using System;
using System.Linq;


namespace Test.Utils.Helper
{
    public class ModelInspector
    {
        public static string ObjectDump<T>(T obj)
        {
            if (obj is null)
                return "null";

            return string.Join(
                Environment.NewLine,
                typeof(T)
                    .GetProperties()
                    .Select(p =>
                    {
                        var value = p.GetValue(obj);
                        return $"{p.Name} = {value}";
                    }));
        }
    }
}
