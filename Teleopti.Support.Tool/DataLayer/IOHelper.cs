using System.Text;
using System.IO;

namespace Teleopti.Support.Tool.DataLayer
{
    public static class IOHelper
    {
        /// <summary>
        /// Writes text to a file
        /// </summary>
        /// <param name="text">Text to write to the file</param>
        /// <param name="fileName">Name of the file to write to</param>
        /// <param name="append">True if content should be added to the file, false if it should be overwritten</param>
        public static void WriteFile(string text, string fileName, bool append)
        {
            using (var writer = new StreamWriter(fileName, append, Encoding.Default))
            {
                writer.Write(text);
                writer.Flush();
            }
        }


        /// <summary>
        /// Reads the entire content of the file
        /// </summary>
        /// <param name="fileName">Name of the file to read</param>
        /// <returns>Returns the content of the file</returns>
        public static string ReadFile(string fileName)
        {
            string txt;
            using (var reader = new StreamReader(fileName, Encoding.Default))
            {
                txt = reader.ReadToEnd();
                //reader.Close();
            }
            return txt;
        }

        /// <summary>
        /// Checks if a directory exists
        /// </summary>
        /// <param name="dirName">Absolute path to the directory</param>
        /// <returns>Returns true if the folder exists</returns>
        public static bool DirectoryExists(string dirName)
        {
            var file = new FileInfo(dirName);
            DirectoryInfo dir = file.Extension.Length < 1 ? new DirectoryInfo(dirName) : file.Directory;
            return dir.Exists;
        }
        /// <summary>
        /// Creates a new directory
        /// </summary>
        /// <param name="dirName">Absolute path of the folder to create</param>
        public static void CreateDirectory(string dirName)
        {
            var dir = new DirectoryInfo(dirName);
            if (!dir.Exists)
                dir.Create();
        }
        /// <summary>
        /// Copy one file from one location to a another location
        /// </summary>
        /// <param name="fileName">The name of the file to copy</param>
        /// <param name="destinationName">Where to copy the file</param>
        /// <param name="overwrite">If true and if the file exists it will be owerwritten</param>
        public static void Copy(string fileName, string destinationName,bool overwrite)
        {
            File.Copy(fileName, destinationName, overwrite);
        }


    }
}

