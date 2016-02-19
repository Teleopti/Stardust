using System.IO;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Support.Library.Config
{
	public class FileConfigurator
	{
		public void Configure(string sourceFile, string targetFile, Tags tags)
		{
			var contents = File.ReadAllText(sourceFile);
			tags.ForReplacing()
				.ForEach(sr =>
				{
					contents = contents.Replace(sr.SearchFor, sr.ReplaceWith);
				});
			File.WriteAllText(targetFile, contents);
		}
	}
}