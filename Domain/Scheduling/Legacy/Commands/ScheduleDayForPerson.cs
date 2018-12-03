using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleDayForPerson : IScheduleDayForPerson
	{
		private readonly Func<IScheduleRangeForPerson> _scheduleRangeFinder;

		public ScheduleDayForPerson(Func<IScheduleRangeForPerson> scheduleRangeFinder)
		{
			_scheduleRangeFinder = scheduleRangeFinder;
		}

		public IScheduleDay ForPerson(IPerson person, DateOnly date)
		{
			return _scheduleRangeFinder().ForPerson(person).ScheduledDay(date);
		}
	}
}