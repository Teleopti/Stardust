namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IStateGroupEventPublisher
	{
		void Publish(StateInfo info);
	}
}