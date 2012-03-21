using System;

namespace Teleopti.Ccc.DBManager.Library
{
	public static class Extensions
	{
		public static string GetName(this DatabaseType type)
		{
			return Enum.GetName(typeof(DatabaseType), type);
		}
	}
}