using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeScheduleDayForPerson : IScheduleDayForPerson
	{
		private readonly IScheduleDay[] _scheduleDays;

		public FakeScheduleDayForPerson(params IScheduleDay[] scheduleDayses)
		{
			_scheduleDays = scheduleDayses;
		}

		public IScheduleDay ForPerson(IPerson person, DateOnly date)
		{
			return _scheduleDays.FirstOrDefault(s => person.Equals(s.Person) && s.DateOnlyAsPeriod.DateOnly == date) ?? ScheduleDayFactory.Create(date, person);
		}
	}
}