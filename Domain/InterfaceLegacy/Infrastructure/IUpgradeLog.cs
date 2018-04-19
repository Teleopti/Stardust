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
}