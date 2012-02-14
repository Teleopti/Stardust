using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ShiftsForTwoWeeks : IUserDataSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var today = DateOnly.Today;
			var firstDay = today.AddDays(-7);
			var lastDay = today.AddDays(7);

			var period = new DateOnlyPeriod(firstDay, lastDay);
			var dayCollection = period.DayCollection();
			var timeZone = user.PermissionInformation.DefaultTimeZone();

			var assignmentRepository = new PersonAssignmentRepository(uow);
			var personDayOffRepository = new PersonDayOffRepository(uow);

			foreach (var date in dayCollection)
			{
				if (date.Date.DayOfWeek == DayOfWeek.Saturday || date.Date.DayOfWeek == DayOfWeek.Sunday)
				{
					var personDayOff = PersonDayOffFactory.CreatePersonDayOff(user, TestData.Scenario, date, TestData.DayOffTemplate);
					personDayOffRepository.Add(personDayOff);
				}
				else
				{
					var baseDate = timeZone.ConvertTimeToUtc(date.Date);
					var layerPeriod = new DateTimePeriod(baseDate.AddHours(20), baseDate.AddHours(28));
					var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, TestData.Scenario);
					assignment.SetMainShift(MainShiftFactory.CreateMainShift(TestData.ActivityPhone, layerPeriod, TestData.ShiftCategory));

					layerPeriod = layerPeriod.ChangeStartTime(TimeSpan.FromHours(3)).ChangeEndTime(TimeSpan.FromHours(-4));
					assignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(TestData.ActivityLunch, layerPeriod));

					assignmentRepository.Add(assignment);
				}
			}

		}
	}
}