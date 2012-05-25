using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDisplayRowCreator
	{
		IList<AgentRestrictionsDisplayRow> Create();
	}

	public class AgentRestrictionsDisplayRowCreator : IAgentRestrictionsDisplayRowCreator
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly IList<IPerson> _persons;
		private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;

		public AgentRestrictionsDisplayRowCreator(ISchedulerStateHolder stateHolder, IList<IPerson> persons, IScheduleMatrixListCreator scheduleMatrixListCreator)
		{
			_stateHolder = stateHolder;
			_persons = persons;
			_scheduleMatrixListCreator = scheduleMatrixListCreator;
		}

		public IList<AgentRestrictionsDisplayRow> Create()	
		{
			var displayRows = new List<AgentRestrictionsDisplayRow>();
			var period = _stateHolder.RequestedPeriod.DateOnly;
			
			foreach (var person in _persons)
			{
				var scheduleDays = new List<IScheduleDay>();

				foreach (var dateOnly in period.DayCollection())
				{
					var virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
					if (virtualSchedulePeriod.DateOnlyPeriod.StartDate == DateTime.MinValue) continue;
					if (!virtualSchedulePeriod.IsValid) continue;

					var scheduleDay = _stateHolder.Schedules[person].ScheduledDay(virtualSchedulePeriod.DateOnlyPeriod.StartDate);
					scheduleDays.Add(scheduleDay);
				}

				if (scheduleDays.Count <= 0) continue;
				var matrixLists = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(scheduleDays);

				foreach (var scheduleMatrixPro in matrixLists)
				{
					var displayRow = new AgentRestrictionsDisplayRow(scheduleMatrixPro) {AgentName = _stateHolder.CommonAgentName(scheduleMatrixPro.Person)};
					displayRows.Add(displayRow);
				}
			}

			return displayRows;
		}
	}
}
