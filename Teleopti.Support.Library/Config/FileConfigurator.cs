using System.IO;

namespace Teleopti.Support.Library.Config
{
	public class FileConfigurator
	{
		public void Configure(string sourceFile, string targetFile, SearchReplaceCollection searchReplaceCollection)
		{
			var contents = File.ReadAllText(sourceFile);

			searchReplaceCollection.ForReplacing()
				.ForEach(sr => { contents = contents.Replace(sr.SearchFor, sr.ReplaceWith); });

			if (File.Exists(targetFile))
			{
				var existingContents = File.ReadAllText(targetFile);
				if (contents == existingContents)
					return;
			}

			File.WriteAllText(targetFile, contents);
		}
	}
}