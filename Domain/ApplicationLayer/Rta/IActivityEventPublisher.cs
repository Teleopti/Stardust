namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IActivityEventPublisher
	{
		void Publish(StateInfo info);
	}
}