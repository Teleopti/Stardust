namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IStateEventPublisher
	{
		void Publish(StateInfo info);
	}
}