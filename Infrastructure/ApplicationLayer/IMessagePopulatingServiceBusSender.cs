namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IMessagePopulatingServiceBusSender
	{
		void Send(object message, bool throwOnNoBus);
	}
}