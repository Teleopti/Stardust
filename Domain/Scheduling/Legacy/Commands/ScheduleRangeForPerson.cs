using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleRangeForPerson : IScheduleRangeForPerson
	{
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;

		public ScheduleRangeForPerson(Func<ISchedulingResultStateHolder> resultStateHolder)
		{
			_resultStateHolder = resultStateHolder;
		}

		public IScheduleRange ForPerson(IPerson person)
		{
			return _resultStateHolder().Schedules[person];
		}
	}
}