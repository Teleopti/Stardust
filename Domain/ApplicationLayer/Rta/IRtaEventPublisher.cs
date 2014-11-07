namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IRtaEventPublisher
	{
		void Publish(StateInfo info);
	}
}