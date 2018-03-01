namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IRtaEventPublisher
	{
		void Publish(Context info);
	}
}