using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IUpgradeLog : IDisposable
	{
		void Write(string message);
		void Write(string message, string level);
	}
}