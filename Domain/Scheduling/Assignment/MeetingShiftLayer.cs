using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MeetingShiftLayer : ShiftLayer
	{
		public virtual ExternalMeeting Meeting { get; protected set; }

		public MeetingShiftLayer(IActivity activity, DateTimePeriod period, ExternalMeeting meeting)
			:base(activity, period)
		{
			Meeting = meeting;
		}

		protected MeetingShiftLayer()
		{
		}
	}
}