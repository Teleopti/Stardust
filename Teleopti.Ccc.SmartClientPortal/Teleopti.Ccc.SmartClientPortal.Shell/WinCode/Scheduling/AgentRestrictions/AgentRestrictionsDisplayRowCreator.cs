using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDisplayRowCreator
	{
		IList<AgentRestrictionsDisplayRow> Create(IList<IPerson> persons);
	}

	public class AgentRestrictionsDisplayRowCreator : IAgentRestrictionsDisplayRowCreator
	{
		private readonly ISchedulerStateHolder _stateHolder;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly MatrixUserLockLocker _locker;

		public AgentRestrictionsDisplayRowCreator(ISchedulerStateHolder stateHolder, MatrixListFactory matrixListFactory, MatrixUserLockLocker locker)
		{
			_stateHolder = stateHolder;
			_matrixListFactory = matrixListFactory;
			_locker = locker;
		}

		public IList<AgentRestrictionsDisplayRow> Create(IList<IPerson> persons)
		{
			if (persons == null) throw new ArgumentNullException("persons");

			var displayRows = new List<AgentRestrictionsDisplayRow>();
			var period = _stateHolder.RequestedPeriod.DateOnlyPeriod;

			foreach (var person in persons)
			{
				var scheduleDays = new List<IScheduleDay>();

				foreach (var dateOnly in period.DayCollection())
				{
					var virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
					if (virtualSchedulePeriod.DateOnlyPeriod.StartDate <= DateOnly.MinValue) continue;
					if (!virtualSchedulePeriod.IsValid) continue;

					var scheduleDay = _stateHolder.Schedules[person].ScheduledDay(virtualSchedulePeriod.DateOnlyPeriod.StartDate);
					scheduleDays.Add(scheduleDay);
				}

				if (scheduleDays.Count <= 0) continue;
				var matrixLists = _matrixListFactory.CreateMatrixListForSelection(_stateHolder.Schedules, scheduleDays);
				_locker.Execute(matrixLists, period);

				foreach (var scheduleMatrixPro in matrixLists)
				{
					var displayRow = new AgentRestrictionsDisplayRow(scheduleMatrixPro) { AgentName = _stateHolder.CommonAgentName(scheduleMatrixPro.Person) };
					displayRows.Add(displayRow);
				}
			}

			return displayRows;
		}
	}
}
