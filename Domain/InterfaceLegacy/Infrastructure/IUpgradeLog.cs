using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IUpgradeLog : IDisposable
	{
		void Write(string message);
		void Write(string message, string level);
	}
}