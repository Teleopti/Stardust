using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MeetingShiftLayer : ShiftLayer
	{
		public virtual Guid MeetingId { get; }

		public MeetingShiftLayer(IActivity activity, DateTimePeriod period, Guid meetingId)
			:base(activity, period)
		{
			MeetingId = meetingId;
		}

		protected MeetingShiftLayer()
		{
		}
	}
}