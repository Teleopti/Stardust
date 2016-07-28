using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	[TestFixture, DomainTest]
	public class ScheduleValidationProviderTest:ISetup
	{
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public ScheduleValidationProvider Target;

		public void Setup(ISystem system,IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<ScheduleValidationProvider>().For<IScheduleValidationProvider>();

			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
		}

		[Test]		
		public void ShouldGetResultForNightlyRestRuleCheckBetweenYesterdayAndToday()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();

			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0),TimeSpan.FromHours(40),TimeSpan.FromHours(8),TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2015,12,30), contract, team));
			PersonRepository.Has(person);

			var today = new DateTime(2016, 1, 2);
			var yesterday = new DateTime(2016, 1, 1);
			var dateTimePeriodToday = new DateTimePeriod(2016,1,2,1,2016,1,2,23);
			var dateTimePeriodYesterday = new DateTimePeriod(2016,1,1,1,2016,1,1,23);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignmentToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriodToday, shiftCategory, scenario);
			var personAssignmentYesterday = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriodYesterday, shiftCategory, scenario);
			ScheduleStorage.Add(personAssignmentToday);
			ScheduleStorage.Add(personAssignmentYesterday);
			
			var results = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016,1,2),
				PersonIds = new []
				{
					person.Id.GetValueOrDefault()
				}
			}, BusinessRuleFlags.NewNightlyRestRule);
			results.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			results.First()
				.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "8:00",
					yesterday.ToShortDateString(),
					today.ToShortDateString(), "2:00"));
		}

		[Test]
		public void ShouldGetResultForNightlyRestRuleCheckBetweenTodayAndTomorrow()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();

			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(40), TimeSpan.FromHours(8), TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("Peter", "peter");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2015, 12, 30), contract, team));
			PersonRepository.Has(person);

			var today = new DateTime(2016, 1, 2);
			var tomorrow = new DateTime(2016, 1, 3);
			var dateTimePeriodToday = new DateTimePeriod(2016, 1, 2, 1, 2016, 1, 2, 23);
			var dateTimePeriodTomorrow = new DateTimePeriod(2016, 1, 3, 1, 2016, 1, 3, 23);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignmentToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriodToday, shiftCategory, scenario);
			var personAssignmentTomorrow = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriodTomorrow, shiftCategory, scenario);
			ScheduleStorage.Add(personAssignmentToday);
			ScheduleStorage.Add(personAssignmentTomorrow);

			var results = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016, 1, 2),
				PersonIds = new []
				{
					person.Id.GetValueOrDefault()
				}
			}, BusinessRuleFlags.NewNightlyRestRule);
			results.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			results.First()
				.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "8:00",
					today.ToShortDateString(),
					tomorrow.ToShortDateString(), "2:00"));
		}

		[Test]
		public void ShouldGetWarningForExceedingMaxWeeklyWorkTime()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();
			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(2), TimeSpan.FromHours(8), TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 1, 1), contract, team));
			PersonRepository.Has(person);

			var dateTimePeriod = new DateTimePeriod(2016, 7, 27, 9, 2016, 7, 27, 12);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriod, shiftCategory, scenario);
			ScheduleStorage.Add(personAssignment);

			var result = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016, 7, 27),
				PersonIds = new []{person.Id.GetValueOrDefault()}
			}, BusinessRuleFlags.NewMaxWeekWorkTimeRule);

			result.Count.Should().Be.EqualTo(1);
			result.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.First()
				.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleMaxWeekWorkTimeErrorMessage, "03:00", "02:00"));
		}

		[Test]
		[Ignore]
		public void ShouldGetWarningForNotMeetingMinWeeklyWorkTime()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();
			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40), TimeSpan.FromHours(40), TimeSpan.FromHours(8), TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 1, 1), contract, team));
			PersonRepository.Has(person);

			var activity = ActivityFactory.CreateActivity("Phone");
			//activity.InWorkTime = true;
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 18, 9, 2016, 7, 18, 10), shiftCategory, scenario);
			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 19, 9, 2016, 7, 19, 10), shiftCategory, scenario);
			var personAssignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 20, 9, 2016, 7, 20, 10), shiftCategory, scenario);
			var personAssignment4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 21, 9, 2016, 7, 21, 10), shiftCategory, scenario);
			var personAssignment5 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 22, 9, 2016, 7, 22, 10), shiftCategory, scenario);
			var personAssignment6 = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person,
				new DateOnly(2016, 7, 23), new DayOffTemplate());
			var personAssignment7 = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person,
				new DateOnly(2016, 7, 24), new DayOffTemplate());
			ScheduleStorage.Add(personAssignment1);
			ScheduleStorage.Add(personAssignment2);
			ScheduleStorage.Add(personAssignment3);
			ScheduleStorage.Add(personAssignment4);
			ScheduleStorage.Add(personAssignment5);
			ScheduleStorage.Add(personAssignment6);
			ScheduleStorage.Add(personAssignment7);

			var result = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016, 7, 18),
				PersonIds = new[] { person.Id.GetValueOrDefault() }
			}, BusinessRuleFlags.MinWeekWorkTimeRule);

			result.Count.Should().Be.EqualTo(1);
			result.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.First()
				.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleMaxWeekWorkTimeErrorMessage, "03:00", "02:00"));
		}
	}
}
