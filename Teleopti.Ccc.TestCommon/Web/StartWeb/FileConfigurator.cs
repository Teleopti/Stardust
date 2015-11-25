using System.Collections.Generic;
using System.IO;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb
{
	public static class FileConfigurator
	{
		public static void ConfigureByTags(string sourceFile, string targetFile, IDictionary<string, string> tags)
		{
			var contents = File.ReadAllText(sourceFile);
			tags.ForEach(p =>
				{
					contents = ReplaceTag(contents, p.Key, p.Value);
				});
			File.WriteAllText(targetFile, contents);
		}

		private static string ReplaceTag(string contents, string tag, string value)
		{
			contents = ReplaceAppSettingTag(contents, tag, value);
			contents = ReplaceXmlCommentTag(contents, tag, value);
			contents = ReplaceSimpleTag(contents, tag, value);
			return contents;
		}

		private static string ReplaceSimpleTag(string contents, string tag, string value)
		{
			var variableTag = string.Format("$({0})", tag);
			return contents.Replace(variableTag, value);
		}

		private static string ReplaceXmlCommentTag(string contents, string tag, string value)
		{
			var appSettingTag = string.Format("<!--$({0})-->", tag);
			return contents.Replace(appSettingTag, value);
		}

		private static string ReplaceAppSettingTag(string contents, string tag, string value)
		{
			var appSettingTag = string.Format("<!--$({0}AppSetting)-->", tag);
			var xml = string.Format(@"<add key=""{0}"" value=""{1}"" />", tag, value);
			return contents.Replace(appSettingTag, xml);
		}
	}
}