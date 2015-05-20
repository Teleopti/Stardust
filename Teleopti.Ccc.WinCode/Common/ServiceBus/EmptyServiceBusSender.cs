using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.WinCode.Common.ServiceBus
{
	public class EmptyServiceBusSender : IServiceBusSender
	{
		public void Dispose()
		{
		}

		public void Send(object message, bool throwOnNoBus)
		{
		}
	}
}