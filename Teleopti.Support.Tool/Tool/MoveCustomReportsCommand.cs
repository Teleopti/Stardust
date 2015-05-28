using System.IO;
using log4net;

namespace Teleopti.Support.Tool.Tool
{
	public class MoveCustomReportsCommand :ISupportCommand
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(MoveCustomReportsCommand));
		public void Execute(ModeFile modeFile)
		{
			if(!modeFile.Type.Equals("DEPLOY"))
				return;
			
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