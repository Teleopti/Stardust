using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
		}

		[Test]
		public void ShouldApproveOvertimeRequest()
		{
			var skill1 = new Domain.Forecasting.Skill("skill1");
			var baseDate = new DateOnly(2016, 12, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriod(baseDate, new[] { skill1 });
			var personRequest = createOvertimeRequest(person, DateOnly.Today.ToDateTimePeriod(TimeZoneInfo.Utc));

			LoggedOnUser.SetFakeLoggedOnUser(person);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
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
