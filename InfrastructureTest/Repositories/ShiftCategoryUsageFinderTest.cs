using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class ShiftCategoryUsageFinderTest : DatabaseTest
	{
		[Test]
		public void ShouldFindUsage()
		{
			var cat = new ShiftCategory("test");
			Session.Save(cat);

			var scenario = new Scenario("default") {DefaultScenario = true};
			Session.Save(scenario);

			var act = new Activity("phone");
			Session.Save(act);

			var pa = new PersonAssignment(LoggedOnPerson, scenario, DateOnly.Today);
			pa.SetShiftCategory(cat);
			pa.AddActivity(act,new TimePeriod(10,14));
			Session.Save(pa);

			var readModelRepository = new ScheduleDayReadModelRepository(CurrUnitOfWork);
			var timeZone = LoggedOnPerson.PermissionInformation.DefaultTimeZone();
			readModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				Date = pa.Date.Date,
				ColorCode = cat.DisplayColor.ToArgb(),
				Label = cat.Description.ShortName,
				StartDateTime = pa.Period.StartDateTimeLocal(timeZone),
				EndDateTime = pa.Period.EndDateTimeLocal(timeZone),
				PersonId = LoggedOnPerson.Id.GetValueOrDefault(),
				NotScheduled = false,
				Workday = true
			});
			Session.Flush();

			var finder = new ShiftCategoryUsageFinder(CurrUnitOfWork);
			var result = finder.Find().Single();

			result.DayOfWeek.Should().Be.EqualTo(pa.Date.DayOfWeek);
			result.StartTime.Should().Be.EqualTo(10.0);
			result.EndTime.Should().Be.EqualTo(14.0);
			result.ShiftCategory.Should().Be.EqualTo(cat.Id.ToString());
		}

		[Test]
		public void ShouldOnlyFindUsageLast180Days()
		{
			var cat = new ShiftCategory("test");
			Session.Save(cat);

			var scenario = new Scenario("default") { DefaultScenario = true };
			Session.Save(scenario);

			var act = new Activity("phone");
			Session.Save(act);

			var pa = new PersonAssignment(LoggedOnPerson, scenario, DateOnly.Today.AddDays(-32));
			pa.SetShiftCategory(cat);
			pa.AddActivity(act, new TimePeriod(10, 14));
			Session.Save(pa);

			var readModelRepository = new ScheduleDayReadModelRepository(CurrUnitOfWork);
			var timeZone = LoggedOnPerson.PermissionInformation.DefaultTimeZone();
			readModelRepository.SaveReadModel(new ScheduleDayReadModel
			{
				Date = pa.Date.Date,
				ColorCode = cat.DisplayColor.ToArgb(),
				Label = cat.Description.ShortName,
				StartDateTime = pa.Period.StartDateTimeLocal(timeZone),
				EndDateTime = pa.Period.EndDateTimeLocal(timeZone),
				PersonId = LoggedOnPerson.Id.GetValueOrDefault(),
				NotScheduled = false,
				Workday = true
			});
			Session.Flush();

			var finder = new ShiftCategoryUsageFinder(CurrUnitOfWork);
			var result = finder.Find();

			result.Should().Be.Empty();
		}
	}
}