﻿using System.IO;
using log4net;

namespace Teleopti.Support.Tool.Tool
{
	public class CustomReportsMover
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(CustomReportsMover));

		public void Execute()
		{			
			const string oldDir = "..\\TeleoptiCCC\\Analytics\\Reports\\Custom\\";

			if (Directory.Exists(oldDir))
			{
				logger.Info("Found " + oldDir);
				const string newDir = "..\\TeleoptiCCC\\Web\\Areas\\Reporting\\Reports\\Custom\\";
				if (!Directory.Exists(newDir))
				{
					Directory.CreateDirectory(newDir);
					logger.Info("Created " + newDir);
				}
				foreach (var file in Directory.GetFiles(oldDir))
				{
					var name = Path.GetFileName(file);
					File.Move(file, newDir + name);
					logger.Info("Moved file " + name);
				}
				
			}
		}
	}
}