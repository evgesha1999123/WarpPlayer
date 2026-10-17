

namespace Player.Utils.File
{
    public class FileValidator
    {
        public static bool IsExist(string? path)
        {
            return path != null ? System.IO.File.Exists(path) : false;
        }
    }
}
