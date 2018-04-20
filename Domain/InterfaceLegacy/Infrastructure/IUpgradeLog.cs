using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IUpgradeLog : IDisposable
	{
		void Write(string message);
		void Write(string message, string level);
	}

	public class NullLog : IUpgradeLog
	{
		public void Write(string message)
		{
		}

		public void Write(string message, string level)
		{
		}

		public void Dispose()
		{
		}
	}
	
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