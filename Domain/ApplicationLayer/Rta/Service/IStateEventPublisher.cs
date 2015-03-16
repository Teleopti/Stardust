namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateEventPublisher
	{
		void Publish(StateInfo info);
	}
}