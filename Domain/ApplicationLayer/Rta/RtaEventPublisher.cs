namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class RtaEventPublisher : IRtaEventPublisher
	{
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IAdherenceEventPublisher _adherenceEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;

		public RtaEventPublisher(IShiftEventPublisher shiftEventPublisher, IAdherenceEventPublisher adherenceEventPublisher, IActivityEventPublisher activityEventPublisher, IStateEventPublisher stateEventPublisher)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
		}

		public void Publish(StateInfo info)
		{
			_shiftEventPublisher.Publish(info);
			_adherenceEventPublisher.Publish(info);
			_activityEventPublisher.Publish(info);
			_stateEventPublisher.Publish(info);
		}
	}
}