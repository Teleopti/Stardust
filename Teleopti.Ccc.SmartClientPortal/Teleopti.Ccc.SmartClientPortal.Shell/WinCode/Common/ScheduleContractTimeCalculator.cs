using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class ScheduleContractTimeCalculator : IScheduleContractTimeCalculator
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IPerson _person;
		private readonly DateOnlyPeriod _dateOnlyPeriod;

		public ScheduleContractTimeCalculator(ISchedulerStateHolder schedulerStateHolder, IPerson person, DateOnlyPeriod dateOnlyPeriod)
		{
			if(schedulerStateHolder == null)
				throw new ArgumentNullException("schedulerStateHolder");

			if(person == null)
				throw new ArgumentNullException("person");

			_schedulerStateHolder = schedulerStateHolder;
			_person = person;
			_dateOnlyPeriod = dateOnlyPeriod;
		}

		public TimeSpan CalculateContractTime()
		{
			var contractTime = TimeSpan.Zero;

			foreach (var scheduleDay in _schedulerStateHolder.Schedules[_person].ScheduledDayCollection(_dateOnlyPeriod))
			{
				contractTime = contractTime.Add(scheduleDay.ProjectionService().CreateProjection().ContractTime());
			}

			return contractTime;
		}
	}
}
