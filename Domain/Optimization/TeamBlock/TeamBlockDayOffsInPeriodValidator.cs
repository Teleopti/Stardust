using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockDayOffsInPeriodValidator
	{
		public bool Validate(ITeamInfo teamInfo, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			foreach (var scheduleMatrixPro in teamInfo.MatrixesForGroup())
			{
				var virtualSchedulePeriod = scheduleMatrixPro.SchedulePeriod;
				var contract = virtualSchedulePeriod.Contract;
				
				if (contract.EmploymentType == EmploymentType.HourlyStaff) continue;	
				
				var targetDaysOff = virtualSchedulePeriod.DaysOff();	
				var dayOffsNow = new List<IScheduleDay>();

				var range = schedulingResultStateHolder.Schedules[virtualSchedulePeriod.Person];
				foreach (var scheduleDay in range.ScheduledDayCollection(virtualSchedulePeriod.DateOnlyPeriod))
				{
					var significant = scheduleDay.SignificantPart();
					if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff) dayOffsNow.Add(scheduleDay);
				}

				if (!(dayOffsNow.Count >= targetDaysOff - contract.NegativeDayOffTolerance && dayOffsNow.Count <= targetDaysOff + contract.PositiveDayOffTolerance)) return false;
			}

			return true;
		}
	}
}
