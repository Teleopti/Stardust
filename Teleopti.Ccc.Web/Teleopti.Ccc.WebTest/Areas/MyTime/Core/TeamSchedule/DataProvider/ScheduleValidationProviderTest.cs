using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
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
	public class ScheduleValidationProviderTest : ISetup
	{
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public ScheduleValidationProvider Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
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
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(40), TimeSpan.FromHours(8), TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("Peter", "peter");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2015, 12, 30), contract, team));
			PersonRepository.Has(person);

			var today = new DateTime(2016, 1, 2);
			var yesterday = new DateTime(2016, 1, 1);
			var dateTimePeriodToday = new DateTimePeriod(2016, 1, 2, 1, 2016, 1, 2, 23);
			var dateTimePeriodYesterday = new DateTimePeriod(2016, 1, 1, 1, 2016, 1, 1, 23);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignmentToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriodToday, shiftCategory, scenario);
			var personAssignmentYesterday = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriodYesterday, shiftCategory, scenario);
			ScheduleStorage.Add(personAssignmentToday);
			ScheduleStorage.Add(personAssignmentYesterday);

			var results = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016, 1, 2),
				PersonIds = new[]
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
				PersonIds = new[]
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
				PersonIds = new[] { person.Id.GetValueOrDefault() }
			}, BusinessRuleFlags.NewMaxWeekWorkTimeRule);

			result.Count.Should().Be.EqualTo(1);
			result.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.First()
				.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleMaxWeekWorkTimeErrorMessage, "03:00", "02:00"));
		}

		[Test]
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
				.Be.EqualTo(string.Format(Resources.BusinessRuleMinWeekWorktimeErrorMessage, "05:00", "40:00"));
		}

		[Test]
		public void ShouldGetWarningForViolatingDayOffRule()
		{
			var scenario = CurrentScenario.Current();
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			var team = TeamFactory.CreateSimpleTeam();
			var contract = PersonContractFactory.CreatePersonContract();
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 1, 1), contract, team));
			PersonRepository.Has(person);

			var personAssignmentWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2016, 7, 19), TimeSpan.FromHours(24), TimeSpan.FromHours(0), TimeSpan.FromHours(12));
			var activity = ActivityFactory.CreateActivity("Phone");

			activity.InWorkTime = true;
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignmentWithShift = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 18, 20, 2016, 7, 19, 4), shiftCategory, scenario);
			ScheduleStorage.Add(personAssignmentWithDayOff);
			ScheduleStorage.Add(personAssignmentWithShift);

			var result = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016, 7, 18),
				PersonIds = new[] { person.Id.GetValueOrDefault() }
			}, BusinessRuleFlags.NewDayOffRule).Single();

			result.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleDayOffErrorMessage2, new DateOnly(2016, 7, 19).ToShortDateString()));
		}

		[Test]
		public void ShouldGetWarningForNotMeetingMinWeeklyRestTimeWithDayOff()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();
			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40), TimeSpan.FromHours(40), TimeSpan.FromHours(8), TimeSpan.FromHours(36));

			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 1, 1), contract, team));
			PersonRepository.Has(person);

			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 18, 9, 2016, 7, 18, 23), shiftCategory, scenario);
			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 19, 9, 2016, 7, 19, 23), shiftCategory, scenario);
			var personAssignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 20, 9, 2016, 7, 20, 23), shiftCategory, scenario);
			var personAssignment4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 21, 9, 2016, 7, 21, 23), shiftCategory, scenario);
			var personAssignment5 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 22, 9, 2016, 7, 22, 23), shiftCategory, scenario);
			var personAssignment6 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 23, 9, 2016, 7, 23, 22), shiftCategory, scenario);

			var personAssignment7 = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person,
				new DateOnly(2016, 7, 24), new DayOffTemplate());

			var personAssignment8 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 25, 9, 2016, 7, 25, 23), shiftCategory, scenario);

			ScheduleStorage.Add(personAssignment1);
			ScheduleStorage.Add(personAssignment2);
			ScheduleStorage.Add(personAssignment3);
			ScheduleStorage.Add(personAssignment4);
			ScheduleStorage.Add(personAssignment5);
			ScheduleStorage.Add(personAssignment6);
			ScheduleStorage.Add(personAssignment7);
			ScheduleStorage.Add(personAssignment8);

			var result = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016, 7, 18),
				PersonIds = new[] { person.Id.GetValueOrDefault() }
			}, BusinessRuleFlags.MinWeeklyRestRule);

			result.Count.Should().Be.EqualTo(1);
			result.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.First()
				.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleWeeklyRestErrorMessage, "36:00"));
		}

		[Test]
		public void ShouldGetWarningForNotMeetingMinWeeklyRestTimeWithoutDayOff()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();
			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40), TimeSpan.FromHours(40), TimeSpan.FromHours(8), TimeSpan.FromHours(11));

			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2016, 1, 1), contract, team));
			PersonRepository.Has(person);

			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InWorkTime = true;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();

			var personAssignmentLastSunday = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 17, 9, 2016, 7, 17, 23), shiftCategory, scenario);

			var personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 18, 9, 2016, 7, 18, 23), shiftCategory, scenario);
			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 19, 9, 2016, 7, 19, 23), shiftCategory, scenario);
			var personAssignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 20, 9, 2016, 7, 20, 23), shiftCategory, scenario);
			var personAssignment4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 21, 9, 2016, 7, 21, 23), shiftCategory, scenario);
			var personAssignment5 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 22, 9, 2016, 7, 22, 23), shiftCategory, scenario);
			var personAssignment6 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 23, 9, 2016, 7, 23, 23), shiftCategory, scenario);
			var personAssignment7 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 24, 9, 2016, 7, 24, 23), shiftCategory, scenario);

			var personAssignmentNextMonday = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2016, 7, 25, 9, 2016, 7, 25, 23), shiftCategory, scenario);

			ScheduleStorage.Add(personAssignmentLastSunday);
			ScheduleStorage.Add(personAssignment1);
			ScheduleStorage.Add(personAssignment2);
			ScheduleStorage.Add(personAssignment3);
			ScheduleStorage.Add(personAssignment4);
			ScheduleStorage.Add(personAssignment5);
			ScheduleStorage.Add(personAssignment6);
			ScheduleStorage.Add(personAssignment7);
			ScheduleStorage.Add(personAssignmentNextMonday);

			var result = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016, 7, 18),
				PersonIds = new[] { person.Id.GetValueOrDefault() }
			}, BusinessRuleFlags.MinWeeklyRestRule);

			result.Count.Should().Be.EqualTo(1);
			result.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.First()
				.Warnings.Single()
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleWeeklyRestErrorMessage, "11:00"));
		}

		[Test]
		public void ShouldReturnOverlappedActivities()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var stickyActivity = ActivityFactory.CreateActivity("Short Break");

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();
		
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,person,
				new DateTimePeriod(2013,11,14,8,2013,11,14,16),
				shiftCategory,scenario);

		
			pa.AddActivity(stickyActivity, new DateTimePeriod(2013,11,14,12,2013,11,14,13));

			ScheduleStorage.Add(pa);
			var input = new CheckActivityLayerOverlapFormData
			{
				PersonIds = new Guid[] { person.Id.Value },
				Date = new DateOnly(2013, 11, 14),
				StartTime = new DateTime(2013, 11, 14, 10, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0)
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value);

			var overlappedLayer = result.Single().OverlappedLayers.Single();
			overlappedLayer.Name.Should().Be.EqualTo(stickyActivity.Name);
			overlappedLayer.StartTime.Should().Be.EqualTo(new DateTime(2013, 11, 14, 12, 0, 0));
			overlappedLayer.EndTime.Should().Be.EqualTo(new DateTime(2013,11,14,13,0,0));
		}

		[Test]
		public void ShouldNotReturnUnderlyingStickyOverlappedActivity()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var stickyActivity = ActivityFactory.CreateActivity("Short Break");

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();
		
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				stickyActivity,person,
				new DateTimePeriod(2013,11,14,12,2013,11,14,13),
				shiftCategory,scenario);

		
			pa.AddActivity(mainActivity,new DateTimePeriod(2013,11,14,8,2013,11,14,16));

			ScheduleStorage.Add(pa);
			var input = new CheckActivityLayerOverlapFormData
			{
				PersonIds = new Guid[] { person.Id.Value },
				Date = new DateOnly(2013, 11, 14),
				StartTime = new DateTime(2013, 11, 14, 10, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0)
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}
	}
}
