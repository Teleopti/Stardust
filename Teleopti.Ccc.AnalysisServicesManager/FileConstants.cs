using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AnalysisServicesManager
{
	public static class FileConstants
	{
		public const string Xml = "xml";
		public const string Xmla = "xmla";

		public static string SearchPatternForFileType(string fileType)
		{
			const string searchPattern = "*.{0}";
			return string.Format(CultureInfo.InvariantCulture, searchPattern, fileType);
		}

		public static IEnumerable<FileInfo> FilesOfTypeFromFolder(string folder, string extension)
		{
			if (!Directory.Exists(folder))
				return Enumerable.Empty<FileInfo>();

			var scriptsDirectoryInfo = new DirectoryInfo(folder);

			var scriptFiles = scriptsDirectoryInfo.GetFiles(SearchPatternForFileType(extension), SearchOption.TopDirectoryOnly);
			return scriptFiles.OrderBy(s => s.FileNameWithoutExtension(extension));
		}
	}
}