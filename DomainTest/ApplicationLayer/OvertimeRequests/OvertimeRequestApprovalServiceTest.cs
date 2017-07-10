using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	[TestFixture]
	public class OvertimeRequestApprovalServiceTest : ISetup
	{
		private OvertimeRequestApprovalService _target;
		private IScheduleDictionary _scheduleDictionary;
		private readonly IScenario _scenario = new Scenario("default") { DefaultScenario = true };

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(_scenario).For<IScenario>();
			system.UseTestDouble<ScheduleDictionaryForTest>().For<IScheduleDictionary>();
		}

		[Test]
		public void ShouldAddOvertimeActvityWhenRequestIsApproved()
		{
			var skill1 = new Domain.Forecasting.Skill("skill1");
			var skill2 = new Domain.Forecasting.Skill("skill2");
			var baseDate = new DateOnly(2016, 12, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriod(baseDate, new[] { skill1, skill2 });

			var requestPeriod = baseDate.ToDateTimePeriod(new TimePeriod(20, 22), person.PermissionInformation.DefaultTimeZone());
			var personRequest = createOvertimeRequest(person, requestPeriod);

			var schedulePeriod = baseDate.ToDateTimePeriod(new TimePeriod(8, 20), person.PermissionInformation.DefaultTimeZone());
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, _scenario, schedulePeriod);
			_scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(_scenario,
				schedulePeriod, assignment);

			_target = new OvertimeRequestApprovalService(_scheduleDictionary, new DoNothingScheduleDayChangeCallBack());

			_target.Approve(personRequest.Request);

			var personAssignment = _scheduleDictionary.SchedulesForDay(baseDate).FirstOrDefault()?.PersonAssignment();

			personAssignment.Should().Not.Be.Null();
			personAssignment.OvertimeActivities().Count().Should().Be(1);
			personAssignment.OvertimeActivities().First().Payload.Should().Be(skill1.Activity);
			personAssignment.OvertimeActivities().First().Period.Should().Be(requestPeriod);
		}

		private IPersonRequest createOvertimeRequest(IPerson person, DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();

			var personRequest = personRequestFactory.CreatePersonRequest(person);
			var overTimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime),
				period);
			personRequest.Request = overTimeRequest;
			return personRequest;
		}
	}
}
