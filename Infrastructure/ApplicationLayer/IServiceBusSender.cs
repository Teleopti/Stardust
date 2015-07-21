using System;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IServiceBusSender : IDisposable
    {
		void Send(bool throwOnNoBus, params object[] message);
    }
}