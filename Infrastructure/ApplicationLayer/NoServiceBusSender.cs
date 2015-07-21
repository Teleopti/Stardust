namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class NoServiceBusSender : IServiceBusSender
	{
		public void Dispose()
		{
		}

		public void Send(bool throwOnNoBus, params object[] message)
		{
		}
	}
}