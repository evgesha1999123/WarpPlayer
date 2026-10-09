

namespace Test.Utils.File
{
    public class FileValidator
    {
        public static bool CheckIsExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static void CheckFormat()
        {

        }
    }
}
