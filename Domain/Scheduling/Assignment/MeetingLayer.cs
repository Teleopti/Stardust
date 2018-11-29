using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MeetingLayer : ILayer<IActivity>
	{
		public MeetingLayer(IActivity activity, DateTimePeriod period)
		{
			Payload = activity;
			Period = period;
		}

		public DateTimePeriod Period { get; }
		public IActivity Payload { get; }
	}
}