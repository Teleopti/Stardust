namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class RtaEventPublisher : IRtaEventPublisher
	{
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IAdherenceEventPublisher _adherenceEventPublisher;

		public RtaEventPublisher(IShiftEventPublisher shiftEventPublisher, IAdherenceEventPublisher adherenceEventPublisher)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
		}

		public void Publish(StateInfo info)
		{
			_shiftEventPublisher.Publish(info);
			_adherenceEventPublisher.Publish(info);
		}
	}
}