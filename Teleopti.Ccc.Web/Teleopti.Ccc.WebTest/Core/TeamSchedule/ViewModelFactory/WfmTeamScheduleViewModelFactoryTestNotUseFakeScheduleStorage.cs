using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Ccc.Web.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[DomainTest]
	public class WfmTeamScheduleViewModelFactoryTestNotUseFakeScheduleStorage : IIsolateSystem, IExtendSystem
	{
		public ITeamScheduleViewModelFactory Target;
		public FakeScenarioRepository CurrentScenario;
		public FakePersonRepository PersonRepo;
		public FakeMeetingRepository MeetingRepo;
		public FakePersonFinderReadOnlyRepository PersonFinderReadOnlyRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public FakeLoggedOnUser FakeLoggedOnUser;


		private IPerson logonUser;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
			extend.AddModule(new TeamScheduleAreaModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePersonFinderReadOnlyRepository>().For<IPersonFinderReadOnlyRepository>();
			isolate.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		private void setUpLogon()
		{
			logonUser = PersonFactory.CreatePerson("Admin", "Admin").WithId();
			FakeLoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			FakeLoggedOnUser.SetFakeLoggedOnUser(logonUser);

		}

		[Test, SetCulture("zh-CN")]
		public void ShouldReturnUnderlyingScheduleSummaryForPersonMeeting()
		{
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var team = TeamFactory.CreateSimpleTeam().WithId();
			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var activity = ActivityFactory.CreateActivity("person meeting");

			MeetingPerson meetingPerson = new MeetingPerson(personInUtc, false);
			var meeting = new Meeting(personInUtc, new List<IMeetingPerson>() { meetingPerson }, "test meeting", "location", "description", activity, scenario);
			meeting.StartDate = date;
			meeting.EndDate = date;
			meeting.StartTime = new TimeSpan(10, 0, 0);
			meeting.EndTime = new TimeSpan(11, 0, 0);
			MeetingRepo.Has(meeting);


			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false,
				CriteriaDictionary = new Dictionary<PersonFinderField, string>()
			});
			var meetingActivity = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.PersonMeetings.Single();
			meetingActivity.Description.Should().Be.EqualTo("test meeting");
			meetingActivity.Start.Should().Be.EqualTo("2018-04-03 10:00");
			meetingActivity.End.Should().Be.EqualTo("2018-04-03 11:00");
			meetingActivity.StartInUtc.Should().Be.EqualTo("2018-04-03 10:00");
			meetingActivity.EndInUtc.Should().Be.EqualTo("2018-04-03 11:00");
		}

		[Test, SetCulture("zh-CN")]
		public void ShouldReturnUnderlyingScheduleSummaryForPersonalActivities()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var period = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("activity");
			var pa = PersonAssignmentFactory.CreateEmptyAssignment(personInUtc, scenario, period);
			pa.AddPersonalActivity(activity, period);
			PersonAssignmentRepository.Has(pa);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var personalActivity = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.PersonalActivities.Single();
			personalActivity.Description.Should().Be.EqualTo("activity");
			personalActivity.Start.Should().Be.EqualTo("2018-04-03 10:00");
			personalActivity.End.Should().Be.EqualTo("2018-04-03 11:00");
			personalActivity.StartInUtc.Should().Be.EqualTo("2018-04-03 10:00");
			personalActivity.EndInUtc.Should().Be.EqualTo("2018-04-03 11:00");
		}


		[Test, SetCulture("zh-CN")]
		public void ShouldReturnUnderlyingScheduleSummaryForPartTimePersonalAbsences()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, new DateTimePeriod(new DateTime(2018, 04, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 17, 0, 0, DateTimeKind.Utc)));
			PersonAssignmentRepository.Has(pa);

			var absencePeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absencePeriod, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var personalAbsence = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.PersonPartTimeAbsences.Single();
			personalAbsence.Description.Should().Be.EqualTo("absence");
			personalAbsence.Start.Should().Be.EqualTo("2018-04-03 10:00");
			personalAbsence.End.Should().Be.EqualTo("2018-04-03 11:00");
			personalAbsence.StartInUtc.Should().Be.EqualTo("2018-04-03 10:00");
			personalAbsence.EndInUtc.Should().Be.EqualTo("2018-04-03 11:00");
		}

		[Test, SetCulture("zh-CN")]
		public void ShouldReturnUnderlyingScheduleSummaryForPartTimePersonalAbsencesIfItNotIntersectAnyShift()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, new DateTimePeriod(new DateTime(2018, 04, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 17, 0, 0, DateTimeKind.Utc)));
			PersonAssignmentRepository.Has(pa);

			var absencePeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 6, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 7, 0, 0, DateTimeKind.Utc));
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absencePeriod, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var personalAbsence = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.PersonPartTimeAbsences.Single();
			personalAbsence.Description.Should().Be.EqualTo("absence");
			personalAbsence.Start.Should().Be.EqualTo("2018-04-03 06:00");
			personalAbsence.End.Should().Be.EqualTo("2018-04-03 07:00");
			personalAbsence.StartInUtc.Should().Be.EqualTo("2018-04-03 06:00");
			personalAbsence.EndInUtc.Should().Be.EqualTo("2018-04-03 07:00");
		}

		[Test, SetCulture("zh-CN")]
		public void ShouldNotReturnUnderlyingScheduleSummaryIfFullDayAbsenceOnYesterdayShiftThatIsOvernightShift()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 7, 24);
			var team = TeamFactory.CreateSimpleTeam().WithId();

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var period = new DateTimePeriod(new DateTime(2018, 07, 23, 20, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 2, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, period);
			PersonAssignmentRepository.Has(pa);


			var absence = AbsenceFactory.CreateAbsence("absence");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, period, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var periodToday = new DateTimePeriod(new DateTime(2018, 07, 24, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 16, 0, 0, DateTimeKind.Utc));
			var paToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, periodToday);
			PersonAssignmentRepository.Has(paToday);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false
			});
			viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.Should().Be.Null();
		}


		[Test, SetCulture("zh-CN")]
		public void ShouldReturnUnderlyingScheduleSummaryForYesterdayWhenIntradayAbsenceOnlyIntersectWithYesterdaySchedule()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 7, 24);
			var team = TeamFactory.CreateSimpleTeam().WithId();

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var period = new DateTimePeriod(new DateTime(2018, 07, 23, 20, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 2, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, period);
			PersonAssignmentRepository.Has(pa);

			var absencePeriod = new DateTimePeriod(new DateTime(2018, 07, 24, 1, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 3, 0, 0, DateTimeKind.Utc));
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absencePeriod, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var periodToday = new DateTimePeriod(new DateTime(2018, 07, 24, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 16, 0, 0, DateTimeKind.Utc));
			var paToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, periodToday);
			PersonAssignmentRepository.Has(paToday);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false
			});
			viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.Should().Be.Null();
		}

		[Test, SetCulture("zh-CN")]
		public void ShouldReturnUnderlyingScheduleSummaryIfPartTimeAbsenceBelongsToYesterdayOTFullDayAbsenceAndIntersectShiftForToday()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 7, 24);
			var team = TeamFactory.CreateSimpleTeam().WithId();

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var period = new DateTimePeriod(new DateTime(2018, 07, 23, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 23, 10, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateEmptyAssignment(personInUtc, scenario, period);
			PersonAssignmentRepository.Has(pa);

			var periodToday = new DateTimePeriod(new DateTime(2018, 07, 24, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 16, 0, 0, DateTimeKind.Utc));
			var paToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, periodToday);
			PersonAssignmentRepository.Has(paToday);

			var fullDayAbsencePeriod = new DateTimePeriod(new DateTime(2018, 07, 23, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 10, 0, 0, DateTimeKind.Utc));
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, fullDayAbsencePeriod, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var partTimeAbsence = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.PersonPartTimeAbsences.Single();
			partTimeAbsence.Description.Should().Be.EqualTo("absence");
			partTimeAbsence.Start.Should().Be.EqualTo("2018-07-23 08:00");
			partTimeAbsence.End.Should().Be.EqualTo("2018-07-24 10:00");
			partTimeAbsence.StartInUtc.Should().Be.EqualTo("2018-07-23 08:00");
			partTimeAbsence.EndInUtc.Should().Be.EqualTo("2018-07-24 10:00");
		}

		[Test, SetCulture("zh-CN")]
		public void ShouldReturnUnderlyingScheduleSummaryIfPartTimeAbsencePeriodExceedPersonAssignmentPeriod()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 7, 24);
			var team = TeamFactory.CreateSimpleTeam().WithId();

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);


			var periodToday = new DateTimePeriod(new DateTime(2018, 07, 24, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 16, 0, 0, DateTimeKind.Utc));
			var paToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, periodToday);
			PersonAssignmentRepository.Has(paToday);

			var absencePeriod = new DateTimePeriod(new DateTime(2018, 07, 24, 15, 0, 0, DateTimeKind.Utc), new DateTime(2018, 07, 24, 17, 0, 0, DateTimeKind.Utc));
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absencePeriod, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var partTimeAbsence = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.PersonPartTimeAbsences.Single();
			partTimeAbsence.Description.Should().Be.EqualTo("absence");
			partTimeAbsence.Start.Should().Be.EqualTo("2018-07-24 15:00");
			partTimeAbsence.End.Should().Be.EqualTo("2018-07-24 17:00");
			partTimeAbsence.StartInUtc.Should().Be.EqualTo("2018-07-24 15:00");
			partTimeAbsence.EndInUtc.Should().Be.EqualTo("2018-07-24 17:00");
		}


		[Test, SetCulture("zh-CN")]
		public void ShouldNotReturnUnderlyingScheduleSummaryForFullDayAbsence()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var team = TeamFactory.CreateSimpleTeam().WithId();

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var period = new DateTimePeriod(new DateTime(2018, 04, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 04, 17, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, period);
			PersonAssignmentRepository.Has(pa);


			var absence = AbsenceFactory.CreateAbsence("absence");
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, period, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var partTimePeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 04, 10, 0, 0, DateTimeKind.Utc));
			var partTimeAbsence = AbsenceFactory.CreateAbsence("parttime absence");
			var personPartTimeAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, partTimePeriod, partTimeAbsence);
			PersonAbsenceRepository.Has(personPartTimeAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnUnderlyingScheduleSummaryWithCorrectDescriptionIfAbsenceIsConfidential()
		{
			PermissionProvider.Enable();

			var date = new DateOnly(2018, 04, 03);
			var scenario = CurrentScenario.Has("Default");
			setUpLogon();

			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));
			personInUtc.WorkflowControlSet = new WorkflowControlSet();
			personInUtc.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2018, 4, 10);

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, date, new PersonAuthorization
			{
				SiteId = team.Site.Id.GetValueOrDefault(),
				TeamId = team.Id.GetValueOrDefault()
			});


			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, new DateTimePeriod(new DateTime(2018, 04, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 17, 0, 0, DateTimeKind.Utc)));
			PersonAssignmentRepository.Has(pa);

			var absencePeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var absence = AbsenceFactory.CreateAbsence("confidential absence");
			absence.Confidential = true;

			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absencePeriod, absence);
			PersonAbsenceRepository.Has(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var personalAbsence = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary.PersonPartTimeAbsences.Single();
			personalAbsence.Description.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
			personalAbsence.Start.Should().Be.EqualTo("2018-04-03 10:00");
			personalAbsence.End.Should().Be.EqualTo("2018-04-03 11:00");
			personalAbsence.StartInUtc.Should().Be.EqualTo("2018-04-03 10:00");
			personalAbsence.EndInUtc.Should().Be.EqualTo("2018-04-03 11:00");
		}

		[Test]
		public void ShouldReturnNullUnderlyingSchedulesSummaryIfScheduleIsUnpublished()
		{
			PermissionProvider.Enable();
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			personInUtc.WorkflowControlSet = new WorkflowControlSet();
			personInUtc.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2018, 4, 1);


			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();

			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));
			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, date, new PersonAuthorization
			{
				SiteId = team.Site.Id.GetValueOrDefault(),
				TeamId = team.Id.GetValueOrDefault()
			});

			var period = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("activity");
			var pa = PersonAssignmentFactory.CreateEmptyAssignment(personInUtc, scenario, period);
			pa.AddPersonalActivity(activity, period);
			PersonAssignmentRepository.Has(pa);


			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var summary = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary;
			summary.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullUnderlyingScheduleSummaryIfNoUnderlyingSchedules()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			personInUtc.WorkflowControlSet = new WorkflowControlSet();
			personInUtc.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2018, 4, 1);


			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var period = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, period);
			PersonAssignmentRepository.Has(pa);
			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = false

			});
			var summary = viewModel.Schedules.FirstOrDefault().UnderlyingScheduleSummary;
			summary.Should().Be.Null();
		}

		[Test]
		public void ShouldNotReturnPersonWhoHasNoShiftButPartialDayAbsenceWhenFilterOnOnlyShowPersonWithAbsencesIsOn()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var period = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, period);
			PersonAbsenceRepository.Add(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = true

			});

			viewModel.Schedules.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReturnPersonWhoHasShiftAndIntraDayAbsenceOutsideOfShiftWhenFilterOnOnlyShowPersonWithAbsencesIsOn()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var assPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, assPeriod);
			PersonAssignmentRepository.Has(pa);
			var absPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 12, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 13, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absPeriod);
			PersonAbsenceRepository.Add(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = true

			});

			viewModel.Schedules.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotReturnPersonWhoHasShiftAndIntraDayAbsenceJustAfterShiftWhenFilterOnOnlyShowPersonWithAbsencesIsOn()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var assPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, assPeriod);
			PersonAssignmentRepository.Has(pa);
			var absPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 13, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absPeriod);
			PersonAbsenceRepository.Add(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = true

			});

			viewModel.Schedules.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnPersonWhoHasShiftAndFullDayAbsenceWhenFilterOnOnlyShowPersonWithAbsencesIsOn()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var assPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 11, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, assPeriod);
			PersonAssignmentRepository.Has(pa);
			var absPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 23, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absPeriod);
			PersonAbsenceRepository.Add(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = true

			});

			viewModel.Schedules.Select(s => s.PersonId).Distinct().Single().Should().Be.EqualTo(personInUtc.Id.ToString());
		}

		[Test]
		public void ShouldReturnPersonWhoHasNoShiftButFullDayAbsenceWhenFilterOnOnlyShowPersonWithAbsencesIsOn()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var absPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 23, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absPeriod);
			PersonAbsenceRepository.Add(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = true

			});

			viewModel.Schedules.Select(s => s.PersonId).Distinct().Single().Should().Be.EqualTo(personInUtc.Id.ToString());
		}

		[Test]
		public void ShouldReturnPersonWhoHasShiftAndIntraDayAbsenceWhenFilterOnOnlyShowPersonWithAbsencesIsOn()
		{
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			var scenario = CurrentScenario.Has("Default");
			var date = new DateOnly(2018, 04, 03);
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			var assPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 17, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, assPeriod);
			PersonAssignmentRepository.Has(pa);
			var absPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 9, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absPeriod);
			PersonAbsenceRepository.Add(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = true

			});

			viewModel.Schedules.Select(s => s.PersonId).Distinct().Single().Should().Be.EqualTo(personInUtc.Id.ToString());
		}

		[Test]
		public void ShouldNotReturnPersonWhoHasUnpublishedShiftAndIntraDayAbsenceWhenFilterOnOnlyShowPersonWithAbsencesIsOn()
		{
			var date = new DateOnly(2018, 04, 03);
			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();

			personInUtc.WorkflowControlSet = new WorkflowControlSet();
			personInUtc.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2018, 4, 1);

			
			var scenario = CurrentScenario.Has("Default");
			
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(date, team));

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, date, new PersonAuthorization
			{
				SiteId = team.Site.Id.GetValueOrDefault(),
				TeamId = team.Id.GetValueOrDefault()
			});


			var assPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 8, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 17, 0, 0, DateTimeKind.Utc));
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, assPeriod);
			PersonAssignmentRepository.Has(pa);
			var absPeriod = new DateTimePeriod(new DateTime(2018, 04, 03, 9, 0, 0, DateTimeKind.Utc), new DateTime(2018, 04, 03, 10, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario, absPeriod);
			PersonAbsenceRepository.Add(personAbsence);

			var viewModel = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				DateInUserTimeZone = date,
				GroupIds = new[] { team.Id.Value },
				CurrentPageIndex = 1,
				PageSize = 20,
				IsOnlyAbsences = true

			});

			viewModel.Schedules.Should().Be.Empty();
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnCorrectDayScheduleSummaryForNotPermittedUnpublishedSchedule()
		{
			var scheduleDate = new DateOnly(2019, 12, 30);

			var personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes").WithId();
			var site = SiteFactory.CreateSiteWithOneTeam().WithId();
			var team = site.TeamCollection.First().WithId();
			personInUtc.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(scheduleDate, team));
			personInUtc.WorkflowControlSet = new WorkflowControlSet();
			personInUtc.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2019, 1, 1);

			PersonRepo.Has(personInUtc);
			PersonFinderReadOnlyRepository.Has(personInUtc);

			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, scheduleDate, new PersonAuthorization
			{
				SiteId = team.Site.Id.GetValueOrDefault(),
				TeamId = team.Id.GetValueOrDefault()
			});

			var scenario = CurrentScenario.Has("Default");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			PersonAssignmentRepository.Has(pa);
			PersonAbsenceRepository.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new SearchSchedulesInput
			{
				GroupIds = new[] { team.Id.Value },
				CriteriaDictionary = searchTerm,
				DateInUserTimeZone = scheduleDate,
				PageSize = 20,
				CurrentPageIndex = 1
			});

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();
			first.DaySchedules[3].Title.Should().Be(null);
			first.DaySchedules[3].IsDayOff.Should().Be.False();
		}

	}
}
