using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDisplayRowCreator
	{
		//IList<AgentRestrictionsDisplayRow> Create();
		IList<AgentRestrictionsDisplayRow> Create(IList<IPerson> persons);
	}

	public class AgentRestrictionsDisplayRowCreator : IAgentRestrictionsDisplayRowCreator
	{
		private readonly ISchedulerStateHolder _stateHolder;
		//private readonly IList<IPerson> _persons;
		private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
		
		public AgentRestrictionsDisplayRowCreator(ISchedulerStateHolder stateHolder, IScheduleMatrixListCreator scheduleMatrixListCreator)
		{
			_stateHolder = stateHolder;
			//_persons = persons;
			_scheduleMatrixListCreator = scheduleMatrixListCreator;
		}

		//public IList<IPerson> TestPrio()
		//{
		//    return new List<IPerson>{_persons[0]};
		//}

		//public IList<IPerson> TestRest()
		//{
		//    var temp = new List<IPerson>();

		//    foreach (var person in _persons)
		//    {
		//        temp.Add(person);	
		//    }

		//    temp.RemoveAt(0);
		//    return temp;
		//}

		//public IList<AgentRestrictionsDisplayRow> Create()	
		//{
		//    var displayRows = new List<AgentRestrictionsDisplayRow>();
		//    var period = _stateHolder.RequestedPeriod.DateOnly;
		//    var schedulePeriodTargetTimeCalculator = new SchedulePeriodTargetTimeTimeCalculator();
		//    var periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
		//    var restrictionExtractor = new RestrictionExtractor(_stateHolder.SchedulingResultState);
			
		//    foreach (var person in _persons)
		//    {
		//        var scheduleDays = new List<IScheduleDay>();

		//        foreach (var dateOnly in period.DayCollection())
		//        {
		//            var virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
		//            if (virtualSchedulePeriod.DateOnlyPeriod.StartDate == DateTime.MinValue) continue;
		//            if (!virtualSchedulePeriod.IsValid) continue;

		//            var scheduleDay = _stateHolder.Schedules[person].ScheduledDay(virtualSchedulePeriod.DateOnlyPeriod.StartDate);
		//            scheduleDays.Add(scheduleDay);
		//        }

		//        if (scheduleDays.Count <= 0) continue;
		//        var matrixLists = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(scheduleDays);
				

		//        foreach (var scheduleMatrixPro in matrixLists)
		//        {
		//            var currentContractTime = TimeSpan.Zero;
		//            var targetTime = schedulePeriodTargetTimeCalculator.TargetTime(scheduleMatrixPro);
		//            var minMax = schedulePeriodTargetTimeCalculator.TargetWithTolerance(scheduleMatrixPro);
		//            //TODO INCLUDESCHEDULING
		//            var currentDayOffs = periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(restrictionExtractor, scheduleMatrixPro, true, false, false);

		//            foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays)
		//            {
		//                IProjectionService projSvc = scheduleDayPro.DaySchedulePart().ProjectionService();
		//                IVisualLayerCollection res = projSvc.CreateProjection();

		//                //if (IncludeScheduling())
		//                currentContractTime = currentContractTime.Add(res.ContractTime());	
		//            }

		//            var displayRow = new AgentRestrictionsDisplayRow(scheduleMatrixPro) 
		//            {
		//                AgentName = _stateHolder.CommonAgentName(scheduleMatrixPro.Person),
		//                ContractCurrentTime = currentContractTime,
		//                ContractTargetTime = targetTime,
		//                CurrentDaysOff = currentDayOffs,
		//                MinMaxTime = minMax,
		//            };

		//            displayRows.Add(displayRow);
		//        }
		//    }

		//    return displayRows;
		//}

		public IList<AgentRestrictionsDisplayRow> Create(IList<IPerson> persons)
		{
			if (persons == null) throw new ArgumentNullException("persons");

			var displayRows = new List<AgentRestrictionsDisplayRow>();
			var period = _stateHolder.RequestedPeriod.DateOnly;
			var schedulePeriodTargetTimeCalculator = new SchedulePeriodTargetTimeTimeCalculator();
			var periodScheduledAndRestrictionDaysOff = new PeriodScheduledAndRestrictionDaysOff();
			var restrictionExtractor = new RestrictionExtractor(_stateHolder.SchedulingResultState);

			foreach (var person in persons)
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
					var currentContractTime = TimeSpan.Zero;
					var targetTime = schedulePeriodTargetTimeCalculator.TargetTime(scheduleMatrixPro);
					var minMax = schedulePeriodTargetTimeCalculator.TargetWithTolerance(scheduleMatrixPro);
					//TODO INCLUDESCHEDULING
					var currentDayOffs = periodScheduledAndRestrictionDaysOff.CalculatedDaysOff(restrictionExtractor, scheduleMatrixPro, true, false, false);

					foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays)
					{
						IProjectionService projSvc = scheduleDayPro.DaySchedulePart().ProjectionService();
						IVisualLayerCollection res = projSvc.CreateProjection();

						//if (IncludeScheduling())
						currentContractTime = currentContractTime.Add(res.ContractTime());
					}

					var displayRow = new AgentRestrictionsDisplayRow(scheduleMatrixPro)
					{
						AgentName = _stateHolder.CommonAgentName(scheduleMatrixPro.Person),
						ContractCurrentTime = currentContractTime,
						ContractTargetTime = targetTime,
						CurrentDaysOff = currentDayOffs,
						MinMaxTime = minMax,
					};

					displayRows.Add(displayRow);
				}
			}

			return displayRows;
		}
	}
}
