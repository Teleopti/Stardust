namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IActivityEventPublisher
	{
		void Publish(StateInfo info);
	}
}