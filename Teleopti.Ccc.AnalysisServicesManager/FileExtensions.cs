using System.IO;

namespace AnalysisServicesManager
{
	public static class FileExtensions
	{
		public static string FileNameWithoutExtension(this FileInfo file, string extension)
		{
			return file.Name.Replace("." + extension, "");
		}
	}
}