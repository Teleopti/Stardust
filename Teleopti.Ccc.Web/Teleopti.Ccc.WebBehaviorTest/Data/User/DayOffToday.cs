using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class DayOffToday : IUserDataSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var dayOff = new PersonDayOff(user, TestData.Scenario, TestData.DayOffTemplate, DateOnly.Today);
			var dayOffRepository = new DayOffRepository(uow);
			dayOffRepository.Add(dayOff);
		}
	}
}