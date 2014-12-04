using System;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IServiceBusSender : IDisposable
    {
		void Send(object message, bool throwOnNoBus);
    }
}