namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class NoEvents : IShiftEventPublisher, IAdherenceEventPublisher, IStateGroupEventPublisher, IActivityEventPublisher
	{
		public void Publish(StateInfo info)
		{
		}
	}
}