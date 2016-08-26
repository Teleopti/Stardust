﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule
{
	[TestFixture, DomainTest]
	public class ScheduleValidationProviderTest : ISetup
	{
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public ScheduleValidationProvider Target;
		public FakeWriteSideRepository<IActivity> ActivityForId; 

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<ScheduleValidationProvider>().For<IScheduleValidationProvider>();
			system.UseTestDouble<FakePersonNameProvider>().For<IPersonNameProvider>();
			system.UseTestDouble<FakeWriteSideRepository<IActivity>>().For<IProxyForId<IActivity>>();

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
			var warning = results.First().Warnings.Single();
			warning.Content
				.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "8:00",
					yesterday.ToShortDateString(),
					today.ToShortDateString(), "2:00"));

			warning.RuleType.Should().Be.EqualTo("NewNightlyRestRule");
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

			var warning = results.First().Warnings.Single();
			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "8:00",
					today.ToShortDateString(),
					tomorrow.ToShortDateString(), "2:00"));
			warning.RuleType.Should().Be.EqualTo("NewNightlyRestRule");
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

			var warning = result.First().Warnings.Single();

			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleMaxWeekWorkTimeErrorMessage, "03:00", "02:00"));

			warning.RuleType.Should().Be.EqualTo("NewMaxWeekWorkTimeRule");
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

			var warning = result.First().Warnings.Single();

			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleMinWeekWorktimeErrorMessage, "05:00", "40:00"));

			warning.RuleType.Should().Be.EqualTo("MinWeekWorkTimeRule");
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

			var warning = result.Warnings.Single();
			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleDayOffErrorMessage2, new DateOnly(2016, 7, 19).ToShortDateString()));
			warning.RuleType.Should().Be.EqualTo("NewDayOffRule");
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

			var warning = result.First().Warnings.Single();

			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleWeeklyRestErrorMessage, "36:00"));

			warning.RuleType.Should().Be.EqualTo("MinWeeklyRestRule");
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

			var warning = result.First().Warnings.Single();

			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleWeeklyRestErrorMessage, "11:00"));

			warning.RuleType.Should().Be.EqualTo("MinWeeklyRestRule");
		}

		[Test]
		public void ShouldReturnOverlappedActivitiesWhenAddActivity()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var stickyActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;
			ActivityForId.Add(mainActivity);
			ActivityForId.Add(stickyActivity);

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
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = mainActivity.Id.GetValueOrDefault(),
				ActivityType =ActivityType.RegularActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value);

			var overlappedLayer = result.Single().OverlappedLayers.Single();
			overlappedLayer.Name.Should().Be.EqualTo(stickyActivity.Name);
			overlappedLayer.StartTime.Should().Be.EqualTo(new DateTime(2013, 11, 14, 12, 0, 0));
			overlappedLayer.EndTime.Should().Be.EqualTo(new DateTime(2013,11,14,13,0,0));
		}
		[Test]
		public void ShouldReturnOverlappedActivitiesWhenAddPersonalActivity()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var stickyActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;
			ActivityForId.Add(mainActivity);
			ActivityForId.Add(stickyActivity);

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
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = mainActivity.Id.GetValueOrDefault(),
				ActivityType =ActivityType.PersonalActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value);

			var overlappedLayer = result.Single().OverlappedLayers.Single();
			overlappedLayer.Name.Should().Be.EqualTo(stickyActivity.Name);
			overlappedLayer.StartTime.Should().Be.EqualTo(new DateTime(2013, 11, 14, 12, 0, 0));
			overlappedLayer.EndTime.Should().Be.EqualTo(new DateTime(2013,11,14,13,0,0));
		}

		[Test]
		public void ShouldNotReturnUnderlyingStickyOverlappedActivityWhenAddActivity()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var stickyActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;
			ActivityForId.Add(stickyActivity);

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
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = stickyActivity.Id.Value,
				ActivityType = ActivityType.RegularActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}
		[Test]
		public void ShouldNotReturnUnderlyingStickyOverlappedActivityWhenAddpersonalActivity()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var stickyActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;
			ActivityForId.Add(stickyActivity);

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
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = stickyActivity.Id.Value,
				ActivityType = ActivityType.PersonalActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}
		[Test]
		public void ShouldNotReturnOverlappedActivityWhenAddActivityOnFullDayAbsence()
		{
			var person = PersonFactory.CreatePersonWithGuid("John", "Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var stickyActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;
			ActivityForId.Add(stickyActivity);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity, person,
				new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 17),
				shiftCategory, scenario);


			pa.AddActivity(stickyActivity, new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 16));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2013, 11, 14, 8, 2013, 11, 14, 17));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);
			var input = new CheckActivityLayerOverlapFormData
			{
				PersonIds = new Guid[] { person.Id.Value },
				Date = new DateOnly(2013, 11, 14),
				StartTime = new DateTime(2013, 11, 14, 10, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = stickyActivity.Id.Value,
				ActivityType = ActivityType.RegularActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}
		[Test]
		public void ShouldNotReturnOverlappedActivityWhenAddingActivityForDayOffDay()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var stickyActivity = ActivityFactory.CreateActivity("Short Break").WithId();

			stickyActivity.AllowOverwrite = false;
			ActivityForId.Add(stickyActivity);

			var scenario = CurrentScenario.Current();
		
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(
				scenario,person,
				new DateOnly(2013,11,14),
				DayOffFactory.CreateDayOff(new Description("Do")));

		
			ScheduleStorage.Add(pa);
			var input = new CheckActivityLayerOverlapFormData
			{
				PersonIds = new Guid[] { person.Id.Value },
				Date = new DateOnly(2013, 11, 14),
				StartTime = new DateTime(2013, 11, 14, 10, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = stickyActivity.Id.Value,
				ActivityType = ActivityType.RegularActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}
		[Test]
		public void ShouldNotReturnOverlappedActivityWhenAddingActivityForEmptyDay()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();

			mainActivity.AllowOverwrite = true;
			ActivityForId.Add(mainActivity);

			var input = new CheckActivityLayerOverlapFormData
			{
				PersonIds = new Guid[] { person.Id.Value },
				Date = new DateOnly(2013, 11, 14),
				StartTime = new DateTime(2013, 11, 14, 10, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = mainActivity.Id.Value,
				ActivityType = ActivityType.RegularActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReturnStickyOverlappedActivityWhenAddActivityToStickyPersonalActivity()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone").WithId();
			var stickyActivity = ActivityFactory.CreateActivity("Short Break").WithId();
	

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;
			ActivityForId.Add(mainActivity);
			ActivityForId.Add(stickyActivity);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();
		
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,person,
				new DateTimePeriod(2013,11,14,8,2013,11,14,17),
				shiftCategory,scenario);

		
			pa.AddPersonalActivity(stickyActivity,new DateTimePeriod(2013,11,14,8,2013,11,14,16));

			ScheduleStorage.Add(pa);
			var input = new CheckActivityLayerOverlapFormData
			{
				PersonIds = new Guid[] { person.Id.Value },
				Date = new DateOnly(2013, 11, 14),
				StartTime = new DateTime(2013, 11, 14, 10, 0, 0),
				EndTime = new DateTime(2013, 11, 14, 14, 0, 0),
				ActivityId = mainActivity.Id.GetValueOrDefault(),
				ActivityType = ActivityType.RegularActivity
			};

			var result = Target.GetActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnOverlappedActivitiesWhenMoveShiftLayer()
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


			pa.AddActivity(stickyActivity,new DateTimePeriod(2013,11,14,12,2013,11,14,13));
			pa.AddActivity(mainActivity,new DateTimePeriod(2013,11,14, 9,2013,11,14,11));
			pa.ShiftLayers.ForEach(x => x.WithId());

			var targetLayer = pa.ShiftLayers.First(layer => layer.Period.StartDateTime == new DateTime(2013, 11, 14, 9, 0, 0));

			ScheduleStorage.Add(pa);
			var input = new CheckMoveActivityLayerOverlapFormData
			{
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> { targetLayer.Id.Value }
					} 
				},
				Date = new DateOnly(2013,11,14),
				StartTime = new DateTime(2013,11,14,11,0,0),			
			};

			var result = Target.GetMoveActivityLayerOverlapCheckingResult(input);

			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value);

			var overlappedLayer = result.Single().OverlappedLayers.Single();
			overlappedLayer.Name.Should().Be.EqualTo(stickyActivity.Name);
			overlappedLayer.StartTime.Should().Be.EqualTo(new DateTime(2013,11,14,12,0,0));
			overlappedLayer.EndTime.Should().Be.EqualTo(new DateTime(2013,11,14,13,0,0));
		}

		[Test]
		public void ShouldReturnOverlappedActivitiesWhenMovePersonalShiftLayerToStickyPersonalActivity()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var anotherActivity = ActivityFactory.CreateActivity("Administration");
			var stickyActivity = ActivityFactory.CreateActivity("Short Break");

			mainActivity.AllowOverwrite = true;
			stickyActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,person,
				new DateTimePeriod(2013,11,14,8,2013,11,14,16),
				shiftCategory,scenario);


			pa.AddPersonalActivity(stickyActivity,new DateTimePeriod(2013,11,14,12,2013,11,14,13));
			pa.AddPersonalActivity(anotherActivity,new DateTimePeriod(2013,11,14, 9,2013,11,14,11));
			pa.ShiftLayers.ForEach(x => x.WithId());

			var targetLayer = pa.ShiftLayers.First(layer => layer.Period.StartDateTime == new DateTime(2013, 11, 14, 9, 0, 0));

			ScheduleStorage.Add(pa);
			var input = new CheckMoveActivityLayerOverlapFormData
			{
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> { targetLayer.Id.Value }
					} 
				},
				Date = new DateOnly(2013,11,14),
				StartTime = new DateTime(2013,11,14,11,0,0),			
			};

			var result = Target.GetMoveActivityLayerOverlapCheckingResult(input);

			result.Single().PersonId.Should().Be.EqualTo(person.Id.Value);

			var overlappedLayer = result.Single().OverlappedLayers.Single();
			overlappedLayer.Name.Should().Be.EqualTo(stickyActivity.Name);
			overlappedLayer.StartTime.Should().Be.EqualTo(new DateTime(2013,11,14,12,0,0));
			overlappedLayer.EndTime.Should().Be.EqualTo(new DateTime(2013,11,14,13,0,0));
		}

		[Test]
		public void ShouldNotReturnUnderlyingStickyOverlappedActivityWhenMoveActivity()
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
			pa.AddActivity(mainActivity,new DateTimePeriod(2013,11,14,9,2013,11,14,11));
			pa.ShiftLayers.ForEach(x => x.WithId());

			var targetLayer = pa.ShiftLayers.First(layer => layer.Period.StartDateTime == new DateTime(2013,11,14,9,0,0));


			ScheduleStorage.Add(pa);
			var input = new CheckMoveActivityLayerOverlapFormData
			{
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> { targetLayer.Id.Value }
					}
				},
				Date = new DateOnly(2013,11,14),
				StartTime = new DateTime(2013,11,14,11,0,0),
			};

			var result = Target.GetMoveActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}


		[Test]
		public void ShouldNotReportStickyOverlappedActivityWhenMoveStickyActivityNormally()
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


			pa.AddActivity(stickyActivity,new DateTimePeriod(2013,11,14,12,2013,11,14,13));			
			pa.ShiftLayers.ForEach(x => x.WithId());

			var targetLayer = pa.ShiftLayers.First(layer => layer.Period.StartDateTime == new DateTime(2013,11,14,12,0,0));

			ScheduleStorage.Add(pa);
			var input = new CheckMoveActivityLayerOverlapFormData
			{
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> { targetLayer.Id.Value }
					}
				},
				Date = new DateOnly(2013,11,14),
				StartTime = new DateTime(2013,11,14,14,0,0),
			};

			var result = Target.GetMoveActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReportOverlappingWarningWhenMoveStickyActivityFromOverlappedAnotherStickyActivityToNewStateWithoutOverlappingStickyActivities()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,person,
				new DateTimePeriod(2013,11,14,8,2013,11,14,16),
				shiftCategory,scenario);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013,11,14,12,2013,11,14,14));
			pa.AddActivity(shortBreakActivity, new DateTimePeriod(2013,11,14,12,2013,11,14,13));
			pa.ShiftLayers.ForEach(x => x.WithId());

			var targetLayer = pa.ShiftLayers.First(layer => layer.Period.EndDateTime == new DateTime(2013,11,14,13,0,0));

			ScheduleStorage.Add(pa);
			var input = new CheckMoveActivityLayerOverlapFormData
			{
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> { targetLayer.Id.Value }
					}
				},
				Date = new DateOnly(2013,11,14),
				StartTime = new DateTime(2013,11,14,10,0,0),
			};

			var result = Target.GetMoveActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReportOverlappingWarningWhenMoveStickyActivityFromOverlappedByAnotherStickyActivityToNewStateWithoutOverlappingStickyActivities()
		{
			var person = PersonFactory.CreatePersonWithGuid("John","Watson");
			PersonRepository.Has(person);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var shortBreakActivity = ActivityFactory.CreateActivity("Short Break");
			var lunchActivity = ActivityFactory.CreateActivity("Lunch");

			mainActivity.AllowOverwrite = true;
			shortBreakActivity.AllowOverwrite = false;
			lunchActivity.AllowOverwrite = false;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var scenario = CurrentScenario.Current();

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				mainActivity,person,
				new DateTimePeriod(2013,11,14,8,2013,11,14,17),
				shiftCategory,scenario);

			pa.AddActivity(lunchActivity, new DateTimePeriod(2013,11,14,12,2013,11,14,14));
			pa.AddActivity(shortBreakActivity, new DateTimePeriod(2013,11,14,12,2013,11,14,13));
			pa.ShiftLayers.ForEach(x => x.WithId());

			var targetLayer = pa.ShiftLayers.First(layer => layer.Period.EndDateTime == new DateTime(2013,11,14,14,0,0));

			ScheduleStorage.Add(pa);
			var input = new CheckMoveActivityLayerOverlapFormData
			{
				PersonActivities = new List<PersonActivityItem>
				{
					new PersonActivityItem
					{
						PersonId = person.Id.Value,
						ShiftLayerIds = new List<Guid> { targetLayer.Id.Value }
					}
				},
				Date = new DateOnly(2013,11,14),
				StartTime = new DateTime(2013,11,14,15,0,0),
			};

			var result = Target.GetMoveActivityLayerOverlapCheckingResult(input);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldGetResultForNotOverwriteLayerRuleCheckWhenStickyActivityIsOverlappedByAnotherActivity()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();

			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0),TimeSpan.FromHours(40),TimeSpan.FromHours(8),TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2015,12,30),contract,team));
			PersonRepository.Has(person);

			var dateTimePeriodToday = new DateTimePeriod(2016,1,2,8,2016,1,2,17);
			var stickyActivity = ActivityFactory.CreateActivity("Short Break");
			stickyActivity.AllowOverwrite = false;
			var normalActivity = ActivityFactory.CreateActivity("Phone");
			normalActivity.AllowOverwrite = true;
			
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(normalActivity, person,dateTimePeriodToday,shiftCategory,scenario);

			var stickyActivityPeriod = new DateTimePeriod(2016, 1, 2, 11, 2016, 1, 2, 13);
			var normalActivityPeriod = new DateTimePeriod(2016, 1, 2, 12, 2016, 1, 2, 14);

			personAssignment.AddActivity(stickyActivity, stickyActivityPeriod);
			personAssignment.AddActivity(normalActivity,normalActivityPeriod);
			
			var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			var loggedOnTimezone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;


			var stickyActvityTimePeriod = stickyActivityPeriod.TimePeriod(loggedOnTimezone);
			var normalActivityTimePeriod = normalActivityPeriod.TimePeriod(loggedOnTimezone);

			ScheduleStorage.Add(personAssignment);
			

			var results = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016,1,2),
				PersonIds = new[]
				{
					person.Id.GetValueOrDefault()
				}
			}, BusinessRuleFlags.NotOverwriteLayerRule);

			results.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());

			var warning = results.First().Warnings.Single();

			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleOverlappingErrorMessage3,
					"Short Break",
					stickyActvityTimePeriod.ToShortTimeString(loggedOnCulture),
					"Phone",
					normalActivityTimePeriod.ToShortTimeString(loggedOnCulture)));

			warning.RuleType.Should().Be.EqualTo("NotOverwriteLayerRule");
		}

		[Test]
		public void ShouldGetResultForNotOverwriteLayerRuleCheckWhenStickyActivityIsOverlappedByPersonalActivity()
		{
			var scenario = CurrentScenario.Current();
			var team = TeamFactory.CreateSimpleTeam();

			var contract = PersonContractFactory.CreatePersonContract();
			contract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0),TimeSpan.FromHours(40),TimeSpan.FromHours(8),TimeSpan.FromHours(40));

			var person = PersonFactory.CreatePersonWithGuid("Peter","peter");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2015,12,30),contract,team));
			PersonRepository.Has(person);

			var dateTimePeriodToday = new DateTimePeriod(2016,1,2,8,2016,1,2,17);
			var stickyActivity = ActivityFactory.CreateActivity("Short Break");
			stickyActivity.AllowOverwrite = false;
			var normalActivity = ActivityFactory.CreateActivity("Phone");
			normalActivity.AllowOverwrite = true;

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory();
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(normalActivity,person,dateTimePeriodToday,shiftCategory,scenario);

			var stickyActivityPeriod = new DateTimePeriod(2016,1,2,11,2016,1,2,13);
			var normalActivityPeriod = new DateTimePeriod(2016,1,2,12,2016,1,2,14);

			personAssignment.AddActivity(stickyActivity,stickyActivityPeriod);
			personAssignment.AddPersonalActivity(normalActivity,normalActivityPeriod);

			var loggedOnCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			var loggedOnTimezone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;


			var stickyActvityTimePeriod = stickyActivityPeriod.TimePeriod(loggedOnTimezone);
			var normalActivityTimePeriod = normalActivityPeriod.TimePeriod(loggedOnTimezone);

			ScheduleStorage.Add(personAssignment);


			var results = Target.GetBusinessRuleValidationResults(new FetchRuleValidationResultFormData
			{
				Date = new DateTime(2016,1,2),
				PersonIds = new[]
				{
					person.Id.GetValueOrDefault()
				}
			},BusinessRuleFlags.NotOverwriteLayerRule);

			results.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());

			var warning = results.First().Warnings.Single();

			warning.RuleType.Should().Be.EqualTo("NotOverwriteLayerRule");
			warning.Content.Should()
				.Be.EqualTo(string.Format(Resources.BusinessRuleOverlappingErrorMessage3,
					"Short Break",
					stickyActvityTimePeriod.ToShortTimeString(loggedOnCulture),
					"Phone",
					normalActivityTimePeriod.ToShortTimeString(loggedOnCulture)));
		}

		[Test]
		public void ShouldGetAllAvailableValidationRuleTypes()
		{
			var ruleFlags = BusinessRuleFlags.None;
			ruleFlags |= BusinessRuleFlags.NewNightlyRestRule;
			ruleFlags |= BusinessRuleFlags.MinWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.NewMaxWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.MinWeeklyRestRule;
			ruleFlags |= BusinessRuleFlags.NewDayOffRule;
			ruleFlags |= BusinessRuleFlags.NotOverwriteLayerRule;

			var ruleTypes = Target.GetAllValidationRuleTypes(ruleFlags);

			ruleTypes.Contains("NewNightlyRestRuleName").Should().Be.True();
			ruleTypes.Contains("MinWeekWorkTimeRuleName").Should().Be.True();
			ruleTypes.Contains("NewMaxWeekWorkTimeRuleName").Should().Be.True();
			ruleTypes.Contains("MinWeeklyRestRuleName").Should().Be.True();
			ruleTypes.Contains("NotOverwriteLayerRuleName").Should().Be.True();
			ruleTypes.Contains("NewDayOffRuleName").Should().Be.True();
		}

	}
}
