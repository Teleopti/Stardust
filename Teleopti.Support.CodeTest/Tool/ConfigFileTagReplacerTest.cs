using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Support.Library.Config;
using Teleopti.Support.Tool.Tool;

namespace Teleopti.Support.CodeTest.Tool
{
	[TestFixture]
	public class ConfigFileTagReplacerTest
	{
		[Test]
		public void ReplaceTagsTest()
		{
			var lst = new List<SearchReplace>() { new SearchReplace { SearchFor = "#SOMETEXTTHATSHOULDNOTBETHERE", ReplaceWith = "NOTHING" } };
			var target = new ConfigFileTagReplacer();
			// should not be any tags to replace, i hope
			target.ReplaceTags("ConfigFiles/ConfigFiles.txt", lst);
		}
	}
}
