namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class NoEvents : IShiftEventPublisher, IAdherenceEventPublisher, IStateEventPublisher, IActivityEventPublisher
	{
		public void Publish(StateInfo info)
		{
		}
	}
}