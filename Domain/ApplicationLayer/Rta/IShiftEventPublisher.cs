namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IShiftEventPublisher
	{
		void Publish(StateInfo info);
	}
}