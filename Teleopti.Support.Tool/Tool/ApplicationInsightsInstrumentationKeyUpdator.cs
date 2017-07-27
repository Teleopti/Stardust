using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Web.Administration;

namespace Teleopti.Support.Tool.Tool
{
	public class ApplicationInsightsInstrumentationKeyUpdator
	{
		public void Update(string instrumentationKey)
		{
			var folders = new HashSet<string>();
			using (var serverManager = new ServerManager())
			{
				foreach (var site in serverManager.Sites)
				{
					foreach (var application in site.Applications)
					{
						if (application.Path.EndsWith("/Web"))
						{
							foreach (var directory in application.VirtualDirectories)
							{
								folders.Add(Path.Combine(directory.PhysicalPath, @"\..\..\.."));
							}
						}
					}
				}
			}

			foreach (var folder in folders.SelectMany(Directory.GetDirectories).ToArray())
			{
				folders.Add(folder);
			}

			const string keyPlaceholder = "<!--$(InstrumentationKey)-->";
			var files = folders.SelectMany(
				f =>
				{
					try
					{
						f = Path.GetFullPath(f);
						return Directory.GetFiles(f, "ApplicationInsights.config", SearchOption.AllDirectories);
					}
					catch (Exception)
					{
						return new string[0];
					}
				});
			foreach (var file in files)
			{
				try
				{
					var contents = File.ReadAllText(file);
					if (contents.Contains(keyPlaceholder))
					{
						File.WriteAllText(file, contents.Replace(keyPlaceholder, instrumentationKey));
					}
				}
				catch (Exception)
				{
				}
			}
		}
	}
}