﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	public class MyTimeWebRequestsShiftTradeViewModelFactoryTestAttribute : MyTimeWebTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<Areas.Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
		}
	}

	[TestFixture, MyTimeWebRequestsShiftTradeViewModelFactoryTest]
	public class RequestsShiftTradeViewModelFactoryTest
	{
		public FakePersonRepository PersonRepository;
		public FakeCurrentScenario CurrentScenario;
		public ITeamRepository TeamRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeLoggedOnUser LoggedOnUser;
		public IRequestsShiftTradeScheduleViewModelFactory Target;
		public Areas.Global.FakePermissionProvider PermissionProvider;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;	

		[Test]
		public void ShouldRetrieveMyScheduleFromRawScheduleDataWhenPublished()
		{
			var scenario = CurrentScenario.Current();
			var me = PersonFactory.CreatePerson("me");
			var team = TeamFactory.CreateSimpleTeam("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			me.AddPersonPeriod(personPeriod);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, me,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13,8,0,0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13,10,0,0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.MySchedule.ScheduleLayers.Count().Should().Be(1);
			result.MySchedule.Name.Should().Be.EqualTo("me@me");
			result.MySchedule.ScheduleLayers[0].Start.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
			result.MySchedule.StartTimeUtc.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
		}

		[Test]
		public void ShouldRetrieveMyScheduleFromRawScheduleDataWhenNotPublished()
		{
			PermissionProvider.Enable();

			PermissionProvider.PublishToDate(new DateOnly(2016, 1, 12));

			var scenario = CurrentScenario.Current();
			var me = PersonFactory.CreatePerson("meUnpublish");
			var team = TeamFactory.CreateSimpleTeam("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			me.AddPersonPeriod(personPeriod);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, me,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13,8,0,0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13,10,0,0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new []{team.Id.GetValueOrDefault()}
			});

			result.MySchedule.ScheduleLayers.Should().Be.Null();
		}

		[Test]
		public void ShouldNeverViewUnpublishedSchedule()
		{
			PermissionProvider.Enable();

			PermissionProvider.PublishToDate(new DateOnly(2016, 1, 12));

			var scenario = CurrentScenario.Current();
			var personUnpublished = PersonFactory.CreatePersonWithGuid("person","Unpublished");
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			personUnpublished.AddPersonPeriod(personPeriod);
			
			PersonRepository.Add(personUnpublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personUnpublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13, 10, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldViewPermittedAndPublishedSchedule()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13, 10, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);

			var possibleTradeSchedule = result.PossibleTradeSchedules.First();

			possibleTradeSchedule.StartTimeUtc.Should().Be(new DateTime(2016, 1, 13, 8, 0, 0));
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
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13, 11, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			var confidentialAbs = AbsenceFactory.CreateAbsence("abs");
			confidentialAbs.Confidential = true;
			var personAbs = PersonAbsenceFactory.CreatePersonAbsence(personPublished, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 10, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 11, 0, 0), DateTimeKind.Utc)), confidentialAbs);
			ScheduleStorage.Add(personAss);
			ScheduleStorage.Add(personAbs);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);

			var possibleTradeSchedule = result.PossibleTradeSchedules.First();

			possibleTradeSchedule.StartTimeUtc.Should().Be(new DateTime(2016, 1, 13, 8, 0, 0));
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
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13, 11, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			var confidentialAbs = AbsenceFactory.CreateAbsence("abs");
			confidentialAbs.Confidential = true;
			var personAbs = PersonAbsenceFactory.CreatePersonAbsence(personPublished, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 10, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 11, 0, 0), DateTimeKind.Utc)), confidentialAbs);
			ScheduleStorage.Add(personAss);
			ScheduleStorage.Add(personAbs);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);

			var possibleTradeSchedule = result.PossibleTradeSchedules.First();

			possibleTradeSchedule.StartTimeUtc.Should().Be(new DateTime(2016, 1, 13, 8, 0, 0));
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

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 13), team);
			personPublished.AddPersonPeriod(personPeriod);
			personWithAbsenceOnDayOff.AddPersonPeriod(personPeriod);
			personWithAbsenceOnly.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);
			PersonRepository.Add(personWithAbsenceOnDayOff);
			PersonRepository.Add(personWithAbsenceOnly);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 13, 17, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			var abs = AbsenceFactory.CreateAbsence("abs");
			var personAbs = PersonAbsenceFactory.CreatePersonAbsence(personPublished, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(personAss);
			ScheduleStorage.Add(personAbs);

			var p2AssWithDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, personWithAbsenceOnDayOff,
				new DateOnly(2016, 1, 13), new DayOffTemplate());
			var p2Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p2AssWithDayOff);
			ScheduleStorage.Add(p2Abs);

			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnly,scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 13, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 13, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 13),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFilterOutOvertimeOnDayOffForPossibleTradedSchedules()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario,personPublished,new DateOnly(2016,1,16),new DayOffTemplate(new Description("dayoff")) );
			personAss.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"),new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)), new MultiplicatorDefinitionSet("a",MultiplicatorType.Overtime));

			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnZeroPageCountWhenNoPossibleTradedSchedule()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, personPublished, new DateOnly(2016, 1, 16), new DayOffTemplate(new Description("dayoff")));
			personAss.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"), new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)), new MultiplicatorDefinitionSet("a", MultiplicatorType.Overtime));

			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PageCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFilterOutFullAbsenceOnContractDayOff()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var personWithAbsenceOnContractDayOff = PersonFactory.CreatePersonWithGuid("_", "_");

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			personWithAbsenceOnContractDayOff.AddPersonPeriod(personPeriod);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personWithAbsenceOnContractDayOff);
			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var abs = AbsenceFactory.CreateAbsence("abs");
			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnContractDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldRetrieveEmptyDays()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var personWithEmptySchedule = PersonFactory.CreatePersonWithGuid("_", "_");

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personWithEmptySchedule.AddPersonPeriod(personPeriod);
			personPublished.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personWithEmptySchedule);
			PersonRepository.Add(personPublished);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			PersonRepository.Add(personWithAbsenceOnContractDayOff);
			PersonRepository.Add(personWithMainShift1);
			PersonRepository.Add(personWithMainShift2);
			PersonRepository.Add(personWithDayoff);
			PersonRepository.Add(personWithEmptySchedule);
			PersonRepository.Add(person2WithEmptySchedule);
			PersonRepository.Add(personWithOvertimeShift);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personWithMainShift1,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personWithMainShift2,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 7, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(person2Ass);

			var personOvertimeAss =
				PersonAssignmentFactory.CreateAssignmentWithOvertimeShift(ActivityFactory.CreateActivity("overtime"),
					personWithOvertimeShift,
					new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 6, 0, 0), DateTimeKind.Utc),
						DateTime.SpecifyKind(new DateTime(2016, 1, 16, 7, 0, 0), DateTimeKind.Utc)), scenario);
			ScheduleStorage.Add(personOvertimeAss);

			var abs = AbsenceFactory.CreateAbsence("abs");
			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnContractDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);

			var personWithDayoffAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, personWithDayoff,
				new DateOnly(2016, 1, 16), new DayOffTemplate());
			ScheduleStorage.Add(personWithDayoffAss);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});
		
			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(6);
			var possibleSchedules = result.PossibleTradeSchedules.ToList();
			possibleSchedules[0].Name.Should().Be.EqualTo("person@overtime");
			possibleSchedules[1].Name.Should().Be.EqualTo("person2@published");
			possibleSchedules[2].Name.Should().Be.EqualTo("person1@published");
			possibleSchedules[3].Name.Should().Be.EqualTo("person3@dayoff");
			possibleSchedules[4].Name.Should().Be.EqualTo("person5@anotherEmpty");
			possibleSchedules[5].Name.Should().Be.EqualTo("person4@empty");
		}

		[Test]
		public void ShouldViewOvertimeIndicatorOnlyForMySchedule()
		{
			var scenario = CurrentScenario.Current();
			var personPublished = PersonFactory.CreatePersonWithGuid("person", "published");
			var me = PersonFactory.CreatePersonWithGuid("me","publised");
			
			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personPublished.AddPersonPeriod(personPeriod);
			me.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personPublished);
			PersonRepository.Add(me);
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc), 
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(personAss);
			var meAss = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(scenario, me,
							new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc), 
								DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20},
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault()}
			});

			result.PossibleTradeSchedules.Single().ScheduleLayers.Single().IsOvertime.Should().Be.False();
			result.MySchedule.ScheduleLayers.Single().IsOvertime.Should().Be.True();
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

			var team = TeamFactory.CreateTeamWithId("team");
			TeamRepository.Add(team);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2016, 1, 16), team);
			personWithAbsenceOnContractDayOff.AddPersonPeriod(personPeriod);
			personWithMainShift1.AddPersonPeriod(personPeriod);
			personWithMainShift2.AddPersonPeriod(personPeriod);
			personWithDayoff.AddPersonPeriod(personPeriod);
			personWithEmptySchedule.AddPersonPeriod(personPeriod);

			PersonRepository.Add(personWithAbsenceOnContractDayOff);
			PersonRepository.Add(personWithMainShift1);
			PersonRepository.Add(personWithMainShift2);
			PersonRepository.Add(personWithDayoff);
			PersonRepository.Add(personWithEmptySchedule);

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personWithMainShift1,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(personAss);

			var person2Ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personWithMainShift2,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 7, 0, 0), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("mainShift"));
			ScheduleStorage.Add(person2Ass);

			var abs = AbsenceFactory.CreateAbsence("abs");
			var p3Abs = PersonAbsenceFactory.CreatePersonAbsence(personWithAbsenceOnContractDayOff, scenario,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 0, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 23, 0, 0), DateTimeKind.Utc)), abs);
			ScheduleStorage.Add(p3Abs);

			var personWithDayoffAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, personWithDayoff,
				new DateOnly(2016, 1, 16), new DayOffTemplate());
			ScheduleStorage.Add(personWithDayoffAss);


			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 2, Take = 2 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});
			result.PageCount.Should().Be.EqualTo(2);
			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(2);
			var possibleSchedules = result.PossibleTradeSchedules.ToList();
			
			possibleSchedules[0].Name.Should().Be.EqualTo("person3@dayoff");
			possibleSchedules[1].Name.Should().Be.EqualTo("person4@empty");
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

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(personAss);
			var meAbs = PersonAbsenceFactory.CreatePersonAbsence(me,scenario,new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
								DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meAbs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(personAss);

			var meDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, me, new DateOnly(2016, 1, 16),
				new DayOffTemplate());

			var meAbs = PersonAbsenceFactory.CreatePersonAbsence(me,scenario,new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
								DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meDayOff);
			ScheduleStorage.Add(meAbs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(personAss);

			var meAbs = PersonAbsenceFactory.CreatePersonAbsence(me,scenario,new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
								DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(meAbs);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
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

			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, personPublished,
				new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
					DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)));
			ScheduleStorage.Add(personAss);

			var meAss = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, me, new DateOnly(2016, 1, 16),
				new DayOffTemplate());
			meAss.AddOvertimeActivity(ActivityFactory.CreateActivity("overtime"), new DateTimePeriod(DateTime.SpecifyKind(new DateTime(2016, 1, 16, 8, 0, 0), DateTimeKind.Utc),
								DateTime.SpecifyKind(new DateTime(2016, 1, 16, 17, 0, 0), DateTimeKind.Utc)),new MultiplicatorDefinitionSet("a",MultiplicatorType.Overtime));
			ScheduleStorage.Add(meAss);

			var result = Target.CreateViewModel(new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 20 },
				ShiftTradeDate = new DateOnly(2016, 1, 16),
				TeamIdList = new[] { team.Id.GetValueOrDefault() }
			});

			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
			result.MySchedule.IsFullDayAbsence.Should().Be.False();
			result.MySchedule.ScheduleLayers.First().IsOvertime.Should().Be.True();
			result.MySchedule.IsDayOff.Should().Be.True();
		}
	}


}
