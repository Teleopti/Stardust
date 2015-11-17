using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class ConsoleLogger : IUpgradeLog
	{
		public void Write(string message)
		{
			Console.Out.WriteLine(message);
		}

		public void Write(string message, string level)
		{
			Write(level + ": " + message);
		}

		public void Dispose()
		{
		}
	}
}