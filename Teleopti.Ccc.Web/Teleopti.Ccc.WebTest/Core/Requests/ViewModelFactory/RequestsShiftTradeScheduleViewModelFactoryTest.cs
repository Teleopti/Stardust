using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	public class MyTimeWebRequestsShiftTradeViewModelFactoryTestAttribute : MyTimeWebTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble(new FakeCommonAgentNameProvider("{LastName} {FirstName}")).For<ICommonAgentNameProvider>();
			isolate.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakePeopleForShiftTradeFinder>().For<IPeopleForShiftTradeFinder>();
		}
	}

	[TestFixture, MyTimeWebRequestsShiftTradeViewModelFactoryTest]
	public class RequestsShiftTradeScheduleViewModelFactoryTest
	{
		public FakePersonRepository PersonRepository;
		public ICurrentScenario CurrentScenario;
		public ITeamRepository TeamRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeLoggedOnUser LoggedOnUser;
		public IRequestsShiftTradeScheduleViewModelFactory Target;
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public ISettingsPersisterAndProvider<NameFormatSettings> Settings;
		public FakePeopleForShiftTradeFinder PeopleForShiftTradeFinder;

		[Test]
		public void ShouldRetrieveMyScheduleFromRawScheduleDataWhenPublished()
		{
			var scenario = CurrentScenario.Current();
			var me = PersonFactory.CreatePerson("me");
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			me.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			me.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var team = TeamFactory.CreateSimpleTeam("team");
			SiteFactory.CreateSimpleSite().AddTeam(team);
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			me.AddPersonPeriod(personPeriod);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			const double periodLengthInHours = 2;
			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(periodLengthInHours));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(me,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.MySchedule.ScheduleLayers.Length.Should().Be(1);
			result.MySchedule.Name.Should().Be.EqualTo("me me");
			result.MySchedule.ScheduleLayers[0].Start.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
			result.MySchedule.StartTimeUtc.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
			result.MySchedule.ContractTimeInMinute.Should().Be(periodLengthInHours*60);
		}

		[Test]
		public void ShouldRetrieveMyScheduleFromRawScheduleDataWhenNotPublishedAndNoPermission()
		{
			PermissionProvider.Enable();

			PermissionProvider.PublishToDate(new DateOnly(2016, 1, 12));

			var scenario = CurrentScenario.Current();
			var me = PersonFactory.CreatePersonWithGuid("me", "Unpublish");
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			me.AddPersonPeriod(personPeriod);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(2));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(me,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.MySchedule.ScheduleLayers.Should().Be.Null();
		}

		[Test]
		public void ShouldSeeMyUnpublisedScheduleWhenIHaveViewUnpublishedSchedulePermission()
		{
			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			PermissionProvider.PublishToDate(new DateOnly(2016, 1, 12));

			var scenario = CurrentScenario.Current();
			var me = PersonFactory.CreatePersonWithGuid("me", "Unpublish");
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			me.AddPersonPeriod(personPeriod);
			PersonRepository.Add(me);

			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(2));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(me,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.MySchedule.ScheduleLayers.Should().Not.Be.Null();
			result.MySchedule.ScheduleLayers.Length.Should().Be(1);
		}

		[Test]
		public void ShouldSeeAgentUnpublisedScheduleWhenLoggedOnUserHasViewUnpublishedSchedulePermission()
		{
			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			PermissionProvider.PublishToDate(new DateOnly(2096, 1, 12));

			var scenario = CurrentScenario.Current();
			var agent = PersonFactory.CreatePersonWithGuid("agent", "Unpublish");
			var me = PersonFactory.CreatePersonWithGuid("me", "me");
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			agent.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			me.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			me.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var team = TeamFactory.CreateTeamWithId("team");
			SiteFactory.CreateSimpleSite("site").AddTeam(team);
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2096, 1, 13), team);
			agent.AddPersonPeriod(personPeriod);
			me.AddPersonPeriod(personPeriod);
			addPerson(agent, team);
			addPerson(me, team);

			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2096, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(2));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});
			result.PossibleTradeSchedules.Count().Should().Be(1);
			result.PossibleTradeSchedules.First().ScheduleLayers.Should().Not.Be.Null();
			result.PossibleTradeSchedules.First().ScheduleLayers.Length.Should().Be(1);
		}

		[Test]
		public void ShouldNotViewUnpublishedScheduleWhenHasNoViewUnpublishedSchedulePermission()
		{
			PermissionProvider.Enable();

			PermissionProvider.PublishToDate(new DateOnly(2016, 1, 12));

			var scenario = CurrentScenario.Current();
			var personUnpublished = PersonFactory.CreatePersonWithGuid("person", "Unpublished");
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			personUnpublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personUnpublished);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(2));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personUnpublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldViewPermittedAndPublishedSchedule()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2096, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);

			addPerson(personPublished, team);

			var startTime = DateTime.SpecifyKind(new DateTime(2096, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(2));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);

			var possibleTradeSchedule = result.PossibleTradeSchedules.First();

			possibleTradeSchedule.StartTimeUtc.Should().Be(new DateTime(2096, 1, 13, 8, 0, 0));
			possibleTradeSchedule.ScheduleLayers.First().LengthInMinutes.Should().Be(120);
		}

		[Test]
		public void ShouldViewConfidentialAbsenceWhenAllowed()
		{
			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.ViewSchedules);

			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var team = TeamFactory.CreateTeam("team", "site").WithId();
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2096, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);

			addPerson(personPublished, team);

			var startTime = DateTime.SpecifyKind(new DateTime(2096, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(3));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			var confidentialAbs = AbsenceFactory.CreateAbsence("abs");
			confidentialAbs.Confidential = true;
			var personAbs = PersonAbsenceFactory.CreatePersonAbsence(personPublished, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2096, 1, 13, 10, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2096, 1, 13, 11, 0, 0), DateTimeKind.Utc)), confidentialAbs);
			ScheduleStorage.Add(personAss);
			ScheduleStorage.Add(personAbs);

			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.ViewSchedules, new DateOnly(startTime), new PersonAuthorization
			{
				SiteId = team.Site.Id,
				TeamId = team.Id
			});

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);

			var possibleTradeSchedule = result.PossibleTradeSchedules.First();

			possibleTradeSchedule.StartTimeUtc.Should().Be(new DateTime(2096, 1, 13, 8, 0, 0));
			possibleTradeSchedule.ScheduleLayers.First().LengthInMinutes.Should().Be(120);
			possibleTradeSchedule.ScheduleLayers.Second().TitleHeader.Should().Be("abs");
		}

		[Test]
		public void ShouldViewConfidentialAbsenceWhenNotAllowed()
		{
			PermissionProvider.Enable();

			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.ViewSchedules);

			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var team = TeamFactory.CreateTeam("team", "site").WithId();
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2096, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);

			addPerson(personPublished, team);

			var startTime = DateTime.SpecifyKind(new DateTime(2096, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(3));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			var confidentialAbs = AbsenceFactory.CreateAbsence("abs");
			confidentialAbs.Confidential = true;
			var personAbs = PersonAbsenceFactory.CreatePersonAbsence(personPublished, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2096, 1, 13, 10, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2096, 1, 13, 11, 0, 0), DateTimeKind.Utc)), confidentialAbs);
			ScheduleStorage.Add(personAss);
			ScheduleStorage.Add(personAbs);

			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.ViewSchedules, new DateOnly(startTime), new PersonAuthorization
			{
				SiteId = team.Site.Id,
				TeamId = team.Id
			});

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);

			var possibleTradeSchedule = result.PossibleTradeSchedules.First();

			possibleTradeSchedule.StartTimeUtc.Should().Be(new DateTime(2096, 1, 13, 8, 0, 0));
			possibleTradeSchedule.ScheduleLayers.First().LengthInMinutes.Should().Be(120);
			possibleTradeSchedule.ScheduleLayers.Second().TitleHeader.Should().Be(ConfidentialPayloadValues.Description.Name);
		}

		[Test]
		public void ShouldFilterOutFullAbsence()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var personWithAbsenceOnDayOff = PersonFactory.CreatePersonWithGuid("p2", "p2");
			var personWithAbsenceOnly = PersonFactory.CreatePersonWithGuid("_", "_");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithAbsenceOnDayOff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithAbsenceOnDayOff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithAbsenceOnDayOff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithAbsenceOnly.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithAbsenceOnly.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithAbsenceOnly.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);
			personWithAbsenceOnDayOff.AddPersonPeriod(personPeriod);
			personWithAbsenceOnly.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);
			PersonRepository.Add(personWithAbsenceOnDayOff);
			PersonRepository.Add(personWithAbsenceOnly);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAbs = PersonAbsenceFactory.CreatePersonAbsence(personPublished, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(personAss);
			ScheduleStorage.Add(personAbs);

			var p2AssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(personWithAbsenceOnDayOff,
				scenario, new DateOnly(2016, 1, 13), new DayOffTemplate());
			var p2Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p2AssWithDayOff);
			ScheduleStorage.Add(p2Abs);

			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnly, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFilterOutOvertimeOnDayOffForPossibleTradedSchedules()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(personPublished,
				scenario, new DateOnly(2016, 1, 16), new DayOffTemplate(new Description("dayoff")));
			personAss.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));

			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnZeroPageCountWhenNoPossibleTradedSchedule()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(personPublished,
				scenario, new DateOnly(2016, 1, 16), new DayOffTemplate(new Description("dayoff")));
			personAss.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));

			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PageCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFilterOutFullAbsenceOnContractDayOff()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var personWithAbsenceOnContractDayOff = PersonFactory.CreatePersonWithGuid("_", "_");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithAbsenceOnContractDayOff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithAbsenceOnContractDayOff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithAbsenceOnContractDayOff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			personWithAbsenceOnContractDayOff.AddPersonPeriod(personPeriod);
			personPublished.AddPersonPeriod(personPeriod);

			addPerson(personWithAbsenceOnContractDayOff, team);
			addPerson(personPublished, team);

			var startTime = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			ScheduleStorage.Add(personAss);

			var abs = AbsenceFactory.CreateAbsence("abs");
			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnContractDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2096, 1, 16, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2096, 1, 16, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRetrieveEmptyDays()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var personWithEmptySchedule = PersonFactory.CreatePersonWithGuid("_", "_");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithEmptySchedule.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithEmptySchedule.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithEmptySchedule.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2096, 1, 16), team);
			personWithEmptySchedule.AddPersonPeriod(personPeriod);
			personPublished.AddPersonPeriod(personPeriod);

			addPerson(personWithEmptySchedule, team);
			addPerson(personPublished, team);

			var startTime = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldRetrieveSortedSchedules()
		{
			var scenario = CurrentScenario.Current();
			var personWithMainShift1 = PersonFactory.CreatePersonWithGuid("person1", "published");
			var personWithMainShift2 = PersonFactory.CreatePersonWithGuid("person2", "published");
			var personWithOvertimeShift = PersonFactory.CreatePersonWithGuid("person", "overtime");
			var personWithAbsenceOnContractDayOff = PersonFactory.CreatePersonWithGuid("_", "_");
			var personWithDayoff = PersonFactory.CreatePersonWithGuid("person3", "dayoff");
			var personWithEmptySchedule = PersonFactory.CreatePersonWithGuid("person4", "empty");
			var person2WithEmptySchedule = PersonFactory.CreatePersonWithGuid("person5", "anotherEmpty");
			personWithMainShift1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithMainShift1.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithMainShift1.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithMainShift2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithMainShift2.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithMainShift2.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithOvertimeShift.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithOvertimeShift.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithOvertimeShift.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithAbsenceOnContractDayOff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithAbsenceOnContractDayOff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithAbsenceOnContractDayOff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithDayoff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithDayoff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithDayoff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithEmptySchedule.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithEmptySchedule.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithEmptySchedule.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			person2WithEmptySchedule.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person2WithEmptySchedule.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person2WithEmptySchedule.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2096, 1, 16), team);
			personWithAbsenceOnContractDayOff.AddPersonPeriod(personPeriod);
			personWithMainShift1.AddPersonPeriod(personPeriod);
			personWithMainShift2.AddPersonPeriod(personPeriod);
			personWithDayoff.AddPersonPeriod(personPeriod);
			personWithEmptySchedule.AddPersonPeriod(personPeriod);
			person2WithEmptySchedule.AddPersonPeriod(personPeriod);
			personWithOvertimeShift.AddPersonPeriod(personPeriod);

			addPerson(personWithAbsenceOnContractDayOff, team);
			addPerson(personWithMainShift1, team);
			addPerson(personWithMainShift2, team);
			addPerson(personWithDayoff, team);
			addPerson(personWithEmptySchedule, team);
			addPerson(person2WithEmptySchedule, team);
			addPerson(personWithOvertimeShift, team);

			var startTime1 = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period1 = new DateTimePeriod(startTime1, startTime1.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift1,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var startTime2 = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 7, 0, 0), DateTimeKind.Utc);
			var period2 = new DateTimePeriod(startTime2, startTime2.AddHours(10));
			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift2,
				scenario, period2, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(person2Ass);

			var personOvertimeAss =
				PersonAssignmentFactory.CreateAssignmentWithOvertimeShift(personWithOvertimeShift, scenario, ActivityFactory.CreateActivity("overtime"), new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 6, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2096, 1, 16, 7, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(personOvertimeAss);

			var abs = AbsenceFactory.CreateAbsence("abs");
			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnContractDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2096, 1, 16, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2096, 1, 16, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);

			var personWithDayoffAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(personWithDayoff,
				scenario, new DateOnly(2096, 1, 16), new DayOffTemplate());
			ScheduleStorage.Add(personWithDayoffAss);

			Settings.Persist(new NameFormatSettings {NameFormatId = (int) NameFormatSetting.LastNameThenFirstName});

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(6);
			var possibleSchedules = result.PossibleTradeSchedules.ToList();
			possibleSchedules[0].Name.Should().Be.EqualTo("published person2");
			possibleSchedules[1].Name.Should().Be.EqualTo("published person1");
			possibleSchedules[2].Name.Should().Be.EqualTo("dayoff person3");
			possibleSchedules[3].Name.Should().Be.EqualTo("anotherEmpty person5");
			possibleSchedules[4].Name.Should().Be.EqualTo("empty person4");
			possibleSchedules[5].Name.Should().Be.EqualTo("overtime person");
		}

		[Test]
		public void ShouldSortEmptyScheduleWithEmptyAssLast()
		{
			var scenario = CurrentScenario.Current();
			var personWithMainShift1 = PersonFactory.CreatePersonWithGuid("person1", "published");
			var personWithMainShift2 = PersonFactory.CreatePersonWithGuid("person2", "published");
			var personWithOvertimeShift = PersonFactory.CreatePersonWithGuid("person", "overtime");
			var personWithAbsenceOnContractDayOff = PersonFactory.CreatePersonWithGuid("_", "_");
			var personWithDayoff = PersonFactory.CreatePersonWithGuid("person3", "dayoff");
			var personWithEmptySchedule = PersonFactory.CreatePersonWithGuid("person4", "empty");
			var person2WithEmptySchedule = PersonFactory.CreatePersonWithGuid("person5", "anotherEmpty");
			personWithMainShift1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithMainShift1.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithMainShift1.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithMainShift2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithMainShift2.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithMainShift2.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithOvertimeShift.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithOvertimeShift.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithOvertimeShift.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithAbsenceOnContractDayOff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithAbsenceOnContractDayOff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithAbsenceOnContractDayOff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithDayoff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithDayoff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithDayoff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithEmptySchedule.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithEmptySchedule.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithEmptySchedule.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			person2WithEmptySchedule.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person2WithEmptySchedule.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person2WithEmptySchedule.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personWithAbsenceOnContractDayOff.AddPersonPeriod(personPeriod);
			personWithMainShift1.AddPersonPeriod(personPeriod);
			personWithMainShift2.AddPersonPeriod(personPeriod);
			personWithDayoff.AddPersonPeriod(personPeriod);
			personWithEmptySchedule.AddPersonPeriod(personPeriod);
			person2WithEmptySchedule.AddPersonPeriod(personPeriod);
			personWithOvertimeShift.AddPersonPeriod(personPeriod);

			addPerson(personWithAbsenceOnContractDayOff, team);
			addPerson(personWithMainShift1, team);
			addPerson(personWithMainShift2, team);
			addPerson(personWithDayoff, team);
			addPerson(personWithEmptySchedule, team);
			addPerson(person2WithEmptySchedule, team);
			addPerson(personWithOvertimeShift, team);

			var personWithEmptyAss = PersonAssignmentFactory.CreatePersonAssignment(personWithEmptySchedule, scenario,
				new DateOnly(2016, 1, 16));
			ScheduleStorage.Add(personWithEmptyAss);

			var startTime1 = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period1 = new DateTimePeriod(startTime1, startTime1.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift1,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var startTime2 = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 7, 0, 0), DateTimeKind.Utc);
			var period2 = new DateTimePeriod(startTime2, startTime2.AddHours(9));
			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift2,
				scenario, period2, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(person2Ass);

			var overtimeStart = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 6, 0, 0), DateTimeKind.Utc);
			var overTimePeriod = new DateTimePeriod(overtimeStart, overtimeStart.AddHours(1));
			var personOvertimeAss =
				PersonAssignmentFactory.CreateAssignmentWithOvertimeShift(personWithOvertimeShift, scenario, ActivityFactory.CreateActivity("overtime"), overTimePeriod);
			ScheduleStorage.Add(personOvertimeAss);

			var abs = AbsenceFactory.CreateAbsence("abs");
			var absenceStart = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 0, 0, 0), DateTimeKind.Utc);
			var absencePeriod = new DateTimePeriod(absenceStart, absenceStart.AddHours(23));
			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnContractDayOff, scenario,
				absencePeriod, abs);
			ScheduleStorage.Add(p3Abs);

			var personWithDayoffAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(personWithDayoff,
				scenario, new DateOnly(2096, 1, 16), new DayOffTemplate());
			ScheduleStorage.Add(personWithDayoffAss);

			Settings.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(6);
			var possibleSchedules = result.PossibleTradeSchedules.ToList();
			possibleSchedules[0].Name.Should().Be.EqualTo("overtime person");
			possibleSchedules[1].Name.Should().Be.EqualTo("published person2");
			possibleSchedules[2].Name.Should().Be.EqualTo("published person1");
			possibleSchedules[3].Name.Should().Be.EqualTo("dayoff person3");
			possibleSchedules[4].Name.Should().Be.EqualTo("anotherEmpty person5");
			possibleSchedules[5].Name.Should().Be.EqualTo("empty person4");
		}

		[Test]
		public void ShouldViewOvertimeIndicatorForMyScheduleAndAgentSchedules()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var me = PersonFactory.CreatePersonWithGuid("me", "publised");
			personPublished.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personPublished.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personPublished.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			SiteFactory.CreateSimpleSite("site").AddTeam(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2096, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);
			me.AddPersonPeriod(personPeriod);

			addPerson(personPublished, team);
			addPerson(me, team);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(personPublished,
				scenario, period);
			ScheduleStorage.Add(personAss);

			var meAss = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(me,
				scenario, new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2096, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2096, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meAss);

			Settings.Persist(new NameFormatSettings { NameFormatId = (int)NameFormatSetting.LastNameThenFirstName });

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2096, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.MySchedule.ScheduleLayers.Single().IsOvertime.Should().Be.True();
			result.PossibleTradeSchedules.Single().ScheduleLayers.Single().IsOvertime.Should().Be.True();
		}

		[Test]
		public void ShouldRetrieveSpecificPageWithSortedSchedules()
		{
			var scenario = CurrentScenario.Current();
			var personWithMainShift1 = PersonFactory.CreatePersonWithGuid("person1", "published");
			var personWithMainShift2 = PersonFactory.CreatePersonWithGuid("person2", "published");
			var personWithAbsenceOnContractDayOff = PersonFactory.CreatePersonWithGuid("_", "_");
			var personWithDayoff = PersonFactory.CreatePersonWithGuid("person3", "dayoff");
			var personWithEmptySchedule = PersonFactory.CreatePersonWithGuid("person4", "empty");
			personWithMainShift1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithMainShift1.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithMainShift1.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithMainShift2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithMainShift2.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithMainShift2.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithAbsenceOnContractDayOff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithAbsenceOnContractDayOff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithAbsenceOnContractDayOff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithAbsenceOnContractDayOff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithAbsenceOnContractDayOff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithAbsenceOnContractDayOff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithDayoff.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithDayoff.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithDayoff.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			personWithEmptySchedule.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			personWithEmptySchedule.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			personWithEmptySchedule.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personWithAbsenceOnContractDayOff.AddPersonPeriod(personPeriod);
			personWithMainShift1.AddPersonPeriod(personPeriod);
			personWithMainShift2.AddPersonPeriod(personPeriod);
			personWithDayoff.AddPersonPeriod(personPeriod);
			personWithEmptySchedule.AddPersonPeriod(personPeriod);

			addPerson(personWithAbsenceOnContractDayOff, team);
			addPerson(personWithMainShift1, team);
			addPerson(personWithMainShift2, team);
			addPerson(personWithDayoff, team);
			addPerson(personWithEmptySchedule, team);

			var startTime1 = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period1 = new DateTimePeriod(startTime1, startTime1.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift1,
				scenario, period1, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			ScheduleStorage.Add(personAss);

			var startTime2 = DateTime.SpecifyKind(new DateTime(2096, 1, 16, 7, 0, 0), DateTimeKind.Utc);
			var period2 = new DateTimePeriod(startTime2, startTime2.AddHours(10));
			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(personWithMainShift2,
				scenario, period2, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(person2Ass);

			var abs = AbsenceFactory.CreateAbsence("abs");
			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnContractDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2096, 1, 16, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2096, 1, 16, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);

			var personWithDayoffAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(personWithDayoff,
				scenario, new DateOnly(2096, 1, 16), new DayOffTemplate());
			ScheduleStorage.Add(personWithDayoffAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 2, Take = 2},
				ShiftTradeDate = new DateOnly(2096, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});
			result.PageCount.Should().Be.EqualTo(2);
			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(2);
			var possibleSchedules = result.PossibleTradeSchedules.ToList();

			possibleSchedules[0].Name.Should().Be.EqualTo("dayoff person3");
			possibleSchedules[1].Name.Should().Be.EqualTo("empty person4");
		}

		[Test]
		public void ShouldReturnEmptyPossibleScheduleListWhenMyScheduleIsFullDayAbsence()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var me = PersonFactory.CreatePersonWithGuid("me", "publised");

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);
			me.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished, scenario, period);
			ScheduleStorage.Add(personAss);
			var meAbs = PersonAbsenceFactory.CreatePersonAbsence(me, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meAbs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
			result.MySchedule.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyPossibleScheduleListWhenMyScheduleIsFullDayAbsenceOnDayOff()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var me = PersonFactory.CreatePersonWithGuid("me", "publised");

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);
			me.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			ScheduleStorage.Add(personAss);

			var meDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(me, scenario,
				new DateOnly(2016, 1, 16), new DayOffTemplate());

			var meAbs = PersonAbsenceFactory.CreatePersonAbsence(me, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meDayOff);
			ScheduleStorage.Add(meAbs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
			result.MySchedule.IsFullDayAbsence.Should().Be.True();
			result.MySchedule.IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyPossibleScheduleListWhenMyScheduleIsFullDayAbsenceOnContractDayOff()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var me = PersonFactory.CreatePersonWithGuid("me", "publised");

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			personPublished.AddPersonPeriod(personPeriod);
			me.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));

			ScheduleStorage.Add(personAss);

			var meAbs = PersonAbsenceFactory.CreatePersonAbsence(me, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meAbs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
			result.MySchedule.IsFullDayAbsence.Should().Be.True();
			result.MySchedule.IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldReturnEmptyPossibleScheduleListWhenMyScheduleIsOvertimeOnDayOff()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var me = PersonFactory.CreatePersonWithGuid("me", "publised");

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);
			me.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var startTime = DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddHours(9));
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(personPublished,
				scenario, period, ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var meAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(me, scenario,
				new DateOnly(2016, 1, 16), new DayOffTemplate());
			meAss.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));
			ScheduleStorage.Add(meAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging {Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] {team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
			result.MySchedule.IsFullDayAbsence.Should().Be.False();
			result.MySchedule.ScheduleLayers.First().IsOvertime.Should().Be.True();
			result.MySchedule.IsDayOff.Should().Be.True();
		}

		private void addPerson(IPerson personWithAbsenceOnContractDayOff, ITeam team)
		{
			PersonRepository.Add(personWithAbsenceOnContractDayOff);
			PeopleForShiftTradeFinder.Has(new PersonAuthorization
			{
				PersonId = personWithAbsenceOnContractDayOff.Id.Value,
				TeamId = team.Id.Value
			});
		}
	}
}