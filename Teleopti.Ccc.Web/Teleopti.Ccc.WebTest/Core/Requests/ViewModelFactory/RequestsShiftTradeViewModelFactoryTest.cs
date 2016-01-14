using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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
			system.UseTestDouble<FakePermissionProvider>().For<IPermissionProvider>();
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
		public IPermissionProvider PermissionProvider;

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
				ShiftTradeDate = new DateOnly(2016, 1, 13)
			});

			result.MySchedule.ScheduleLayers.Count().Should().Be(1);
			result.MySchedule.Name.Should().Be.EqualTo("me@me");
			result.MySchedule.ScheduleLayers[0].Start.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
			result.MySchedule.StartTimeUtc.Should().Be.EqualTo(new DateTime(2016, 1, 13, 8, 0, 0));
		}

		[Test]
		public void ShouldRetrieveMyScheduleFromRawScheduleDataWhenNotPublished()
		{
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
				ShiftTradeDate = new DateOnly(2016, 1, 13)
			});

			result.MySchedule.ScheduleLayers.Should().Be.Null();
		}
	}


}
