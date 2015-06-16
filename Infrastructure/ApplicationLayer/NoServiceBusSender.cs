namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class NoServiceBusSender : IServiceBusSender
	{
		public void Dispose()
		{
		}

		public void Send(object message, bool throwOnNoBus)
		{
		}
	}
}