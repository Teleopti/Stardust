using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReplaceString
{
	public class Program
	{
		private const string Teleopti1 =
			"load.php@debug=false&amp;lang=en&amp;modules=mediawiki.legacy.commonPrint,shared%257Cskins.vector&amp;only=styles&amp;printable=1&amp;skin=vector&amp;%252A";

		private const string Teleopti2 =
			"load.php@debug=false&amp;lang=en&amp;modules=mediawiki.legacy.commonPrint,shared%257Cskins.vector&amp;only=styles&amp;skin=vector&amp;%252A";

		private const string Teleopti3 =
			"load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=scripts&amp;printable=1&amp;skin=vector&amp;%252A";

		private const string Teleopti4 =
			"load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=scripts&amp;skin=vector&amp;%252A";

		private const string Teleopti5 =
			"load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=styles&amp;printable=1&amp;skin=vector&amp;%252A";

		private const string Teleopti6 =
			"load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=styles&amp;skin=vector&amp;%252A";

		private const string Teleopti7 =
			"load.php@debug=false&amp;lang=en&amp;modules=skins.vector&amp;only=scripts&amp;printable=1&amp;skin=vector&amp;%252A";

		private const string Teleopti8 =
			"load.php@debug=false&amp;lang=en&amp;modules=skins.vector&amp;only=scripts&amp;skin=vector&amp;%252A";

		private const string Teleopti9 =
			"load.php@debug=false&amp;lang=en&amp;modules=startup&amp;only=scripts&amp;printable=1&amp;skin=vector&amp;%252A";

		private const string Teleopti10 =
			"load.php@debug=false&amp;lang=en&amp;modules=startup&amp;only=scripts&amp;skin=vector&amp;%252A";

		private static int Main(string[] args)
		{
			var appStatus = 0;
			var path = args[0];
			var files = Directory.GetFiles(path);
			var directories = Directory.GetDirectories(path);
			string[] regExps = {"F01%253A[A-Za-z0-9]*\\+[A-Za-z0-9]*", "F02%253A[A-Za-z0-9]*\\+[A-Za-z0-9]*"};

			try
			{
				foreach (var file in files)
					ReplaceInFile(file, regExps, false);

				ReplaceFileNames(files, path);

				foreach (var file in directories.SelectMany(Directory.GetFiles))
					ReplaceInFile(file, regExps, true);

				foreach (var directory in directories.Where(dir => dir.Contains("+")))
				{
					var newDir = directory.Replace("+", "_");
					Directory.Move(directory, newDir);
				}
			}
			catch (Exception)
			{
				appStatus = 4;
				return appStatus;
			}

			return appStatus;
		}

		private static void ReplaceFileNames(IEnumerable<string> files, string path)
		{
			foreach (var file in files)
			{
				if (file.Contains("+"))
				{
					var newfile = file.Replace("+", "_");
					File.Move(file, newfile);
				}

				if (!file.Contains("load.php")) continue;

				var index = file.LastIndexOf("\\", StringComparison.Ordinal);

				if (index == -1) continue;

				var fileName = file.Substring(index + 1, file.Length - index - 1);
				switch (fileName)
				{
					case
						"load.php@debug=false&lang=en&modules=mediawiki.legacy.commonPrint,shared%7Cskins.vector&only=styles&printable=1&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti1.css");
						break;
					case
						"load.php@debug=false&lang=en&modules=mediawiki.legacy.commonPrint,shared%7Cskins.vector&only=styles&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti2.css");
						break;
					case "load.php@debug=false&lang=en&modules=site&only=scripts&printable=1&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti3.css");
						break;
					case "load.php@debug=false&lang=en&modules=site&only=scripts&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti4.css");
						break;
					case "load.php@debug=false&lang=en&modules=site&only=styles&printable=1&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti5.css");
						break;
					case "load.php@debug=false&lang=en&modules=site&only=styles&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti6.css");
						break;
					case "load.php@debug=false&lang=en&modules=skins.vector&only=scripts&printable=1&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti7.css");
						break;
					case "load.php@debug=false&lang=en&modules=skins.vector&only=scripts&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti8.css");
						break;
					case "load.php@debug=false&lang=en&modules=startup&only=scripts&printable=1&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti9.css");
						break;
					case "load.php@debug=false&lang=en&modules=startup&only=scripts&skin=vector&%2A":
						File.Move(file, path + "\\" + "teleopti10.css");
						break;
				}
			}
		}

		public static void ReplaceInFile(string filePath, string[] searchText, bool injectJavascript)
		{
			string content;
			using (var reader = new StreamReader(filePath))
			{
				content = reader.ReadToEnd();
				reader.Close();
			}
			if (content.Contains("load.php"))
			{
				content = content.Replace(Teleopti1, "teleopti1.css");
				content = content.Replace(Teleopti2, "teleopti2.css");
				content = content.Replace(Teleopti3, "teleopti3.css");
				content = content.Replace(Teleopti4, "teleopti4.css");
				content = content.Replace(Teleopti5, "teleopti5.css");
				content = content.Replace(Teleopti6, "teleopti6.css");
				content = content.Replace(Teleopti7, "teleopti7.css");
				content = content.Replace(Teleopti8, "teleopti8.css");
				content = content.Replace(Teleopti9, "teleopti9.css");
				content = content.Replace(Teleopti10, "teleopti10.css");
			}

			foreach (var s in searchText)
			{
				var regExp = new Regex(s);
				foreach (Match match in regExp.Matches(content))
				{
					var matchStr = match.ToString();
					var replaceStr = matchStr.Replace("+", "_");
					content = Regex.Replace(content, matchStr.Replace("+", "\\+"), replaceStr);
				}
			}

			if (injectJavascript)
			{
				const string script =
					@"<script src='..\DynamicLinkReplace.js' type='text/javascript'></script>
<script type='text/javascript'> onload = replaceLinks;</script>
</head>";
				content = content.Replace("</head>", script);
			}

			using (var writer = new StreamWriter(filePath))
			{
				writer.Write(content);
				writer.Close();
			}
		}
	}
}
