using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class ScheduleContractTimeCalculator : IScheduleContractTimeCalculator
	{
		private readonly IScheduleDictionary _schedules;
		private readonly IPerson _person;
		private readonly DateOnlyPeriod _dateOnlyPeriod;

		public ScheduleContractTimeCalculator(IScheduleDictionary schedules, IPerson person, DateOnlyPeriod dateOnlyPeriod)
		{
			_schedules = schedules;
			_person = person;
			_dateOnlyPeriod = dateOnlyPeriod;
		}

		public TimeSpan CalculateContractTime()
		{
			var contractTime = TimeSpan.Zero;

			foreach (var scheduleDay in _schedules[_person].ScheduledDayCollection(_dateOnlyPeriod))
			{
				contractTime = contractTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
			}

			return contractTime;
		}
	}
}
