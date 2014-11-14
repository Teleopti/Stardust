using System;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IServiceBusSender : IDisposable
    {
		bool EnsureBus();
		void Send(object message);
    }
}