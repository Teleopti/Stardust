using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class StateInfo
	{
		public IList<ScheduleLayer> ScheduleLayers { get; set; }
		public IActualAgentState PreviousState { get; set; }
		public IActualAgentState NewState { get; set; }

		public bool IsScheduled { get; set; }
		public bool WasScheduled { get; set; }
		public DateTime CurrentShiftStartTime { get; set; }
		public DateTime CurrentShiftEndTime { get; set; }
		public ScheduleLayer CurrentActivity { get; set; }
		public ScheduleLayer NextActivityInShift { get; set; }
		public ScheduleLayer PreviousActivity { get; set; }

		public DateTime PreviousShiftEndTime { get; set; }
		public DateTime PreviousShiftStartTime { get; set; }

		public bool Send
		{
			get
			{
				return !NewState.ScheduledId.Equals(PreviousState.ScheduledId) ||
					   !NewState.ScheduledNextId.Equals(PreviousState.ScheduledNextId) ||
					   !NewState.AlarmId.Equals(PreviousState.AlarmId) ||
					   !NewState.StateId.Equals(PreviousState.StateId) ||
					   !NewState.ScheduledNext.Equals(PreviousState.ScheduledNext) ||
					   !NewState.NextStart.Equals(PreviousState.NextStart)
					;
			}
		}

		public string DataSource { get; set; }
	}
}