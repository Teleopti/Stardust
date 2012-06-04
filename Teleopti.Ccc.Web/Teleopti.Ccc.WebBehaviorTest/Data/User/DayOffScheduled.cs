using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class DayOffScheduled : IUserDataSetup
	{

		private readonly int _weekday;
		public IDayOffTemplate DayOffTemplate = TestData.DayOffTemplate;

		public DayOffScheduled(int weekday)
		{
			_weekday = weekday;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = new DateOnly(DateHelper.GetFirstDateInWeek(DateTime.Now.Date, cultureInfo).AddDays(_weekday));
			var dayOff = new PersonDayOff(user, TestData.Scenario, DayOffTemplate, date);
			var dayOffRepository = new DayOffRepository(uow);
			dayOffRepository.Add(dayOff);
		}
	}
}