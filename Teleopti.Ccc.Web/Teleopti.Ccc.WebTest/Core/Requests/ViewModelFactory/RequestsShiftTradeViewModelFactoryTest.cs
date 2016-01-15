using System;
using System.Linq;
using Autofac.Core;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
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
			system.UseTestDouble<FakeScheduleRepository>().For<IScheduleRepository>();
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
		public IScheduleRepository ScheduleRepository;
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
			ScheduleRepository.Add(personAss);

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
			ScheduleRepository.Add(personAss);

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
			ScheduleRepository.Add(personAss);


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
			ScheduleRepository.Add(personAss);


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
			ScheduleRepository.Add(personAss);
			ScheduleRepository.Add(personAbs);


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
			ScheduleRepository.Add(personAss);
			ScheduleRepository.Add(personAbs);


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

	}


}
