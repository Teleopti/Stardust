using System.IO;
using Teleopti.Support.Library;

namespace Teleopti.Ccc.DBManager.Library
{
	public static class ScriptPathExtensions
	{
		public static string ScriptFilePath(this string path, DatabaseType type)
		{
			var fileName = type.GetName() + ".sql";
			return Path.Combine(path, fileName);
		}
	}
}