using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace AnalysisServicesManager
{
    public class FileHandler
    {
        public string FileAsString { private set; get; }

        public FileHandler(string filePath)
        {

			using (var reader = new StreamReader(filePath, System.Text.Encoding.Unicode))
            {
                FileAsString = reader.ReadToEnd();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void LogError(string message, string stackTrace)
        {
            string logText = DateTime.Now.ToString(CultureInfo.CurrentCulture) + Environment.NewLine;
            logText += message + Environment.NewLine;
            logText += stackTrace + Environment.NewLine;

            var fileNameUri = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) + @"\ErrorLog.txt";
            var fileName = (new Uri(fileNameUri)).LocalPath;

            StreamWriter sr;
            try
            {
                sr = File.AppendText(fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create Error log.");
                Console.WriteLine(e.Message);
                return;
            }
            
            sr.WriteLine(logText);
            sr.Close();
            Console.WriteLine(string.Concat("Error log created in file '", fileName, "'."));
        }
    }
}