using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	[TestFixture]
	public class OvertimeRequestProcessorTest : ISetup
	{
		public IOvertimeRequestProcessor Target;
		public IPersonRequestRepository PersonRequestRepository;
		public FakeCurrentScenario CurrentScenario;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public const int MinimumApprovalThresholdTimeInMinutes = 15;
		public INow Now;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
			system.UseTestDouble(new ThisIsNow(new DateTime(2017, 07, 12, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
		}

		[Test]
		public void ShouldApproveOvertimeRequest()
		{
			var skill1 = new Domain.Forecasting.Skill("skill1");
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(Now.UtcDateTime()), new[] { skill1 });
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes + 1);

			var personRequest = createOvertimeRequest(person, new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyOvertimeRequestWhenItsStartTimeIsWithinUpcoming15Mins()
		{
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var skill1 = new Domain.Forecasting.Skill("skill1");
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(Now.UtcDateTime()), new[] { skill1 });
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes - 1);

			var personRequest = createOvertimeRequest(person, new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(string.Format(Resources.OvertimeRequestDenyReasonExpired,TimeZoneHelper.ConvertFromUtc(requestStartTime,timeZoneInfo), MinimumApprovalThresholdTimeInMinutes));
		}

		private IPersonRequest createOvertimeRequest(IPerson person, DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();

			var personRequest = personRequestFactory.CreatePersonRequest(person);
			var overTimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime),
				period);
			personRequest.Request = overTimeRequest;
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}
	}
}
