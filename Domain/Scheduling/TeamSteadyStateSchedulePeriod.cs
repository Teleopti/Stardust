using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateSchedulePeriod
	{
		bool SchedulePeriodEquals(IVirtualSchedulePeriod schedulePeriod, IScheduleMatrixPro scheduleMatrixPro);
	}

	public class TeamSteadyStateSchedulePeriod : ITeamSteadyStateSchedulePeriod
	{
		private readonly IVirtualSchedulePeriod _schedulePeriod;
		private readonly ISchedulePeriodTargetTimeCalculator _targetTimeCalculator;
		private readonly IScheduleMatrixPro _scheduleMatrixPro;

		public TeamSteadyStateSchedulePeriod(IVirtualSchedulePeriod schedulePeriod, ISchedulePeriodTargetTimeCalculator targetTimeCalculator, IScheduleMatrixPro scheduleMatrixPro)
		{
			_schedulePeriod = schedulePeriod;
			_targetTimeCalculator = targetTimeCalculator;
			_scheduleMatrixPro = scheduleMatrixPro;
		}

		public bool SchedulePeriodEquals(IVirtualSchedulePeriod schedulePeriod, IScheduleMatrixPro scheduleMatrixPro)	
		{
			if(schedulePeriod == null) throw new ArgumentNullException("schedulePeriod");
			if(scheduleMatrixPro == null) throw new ArgumentNullException("scheduleMatrixPro");

			if (!_schedulePeriod.DateOnlyPeriod.Equals(schedulePeriod.DateOnlyPeriod)) 
				return false;
			if (!_schedulePeriod.PeriodType.Equals(schedulePeriod.PeriodType)) 
				return false;
			if (!_schedulePeriod.DaysOff().Equals(schedulePeriod.DaysOff())) 
				return false;

			var targetTime1 = _targetTimeCalculator.TargetTime(_scheduleMatrixPro);
			var targetTime2 = _targetTimeCalculator.TargetTime(scheduleMatrixPro);

			if (!targetTime1.Equals(targetTime2))
				return false;
			//if (!_schedulePeriod.PeriodTarget().Equals(schedulePeriod.PeriodTarget())) 
			//    return false;
			if (!_schedulePeriod.AverageWorkTimePerDay.Equals(schedulePeriod.AverageWorkTimePerDay)) 
				return false;
			var firstTargetTime = _targetTimeCalculator.TargetWithTolerance(_scheduleMatrixPro);
			var secondTargetTime = _targetTimeCalculator.TargetWithTolerance(scheduleMatrixPro);
			if (!firstTargetTime.Equals(secondTargetTime)) 
				return false;

			return true;
		}
	}
}
