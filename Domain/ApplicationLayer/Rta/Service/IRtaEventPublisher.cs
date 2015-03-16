namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IRtaEventPublisher
	{
		void Publish(StateInfo info);
	}
}