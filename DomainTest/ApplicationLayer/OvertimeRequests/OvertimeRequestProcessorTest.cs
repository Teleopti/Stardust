using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
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
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
		}

		[Test]
		public void ShouldApproveOvertimeRequest()
		{

			var personRequestFactory = new PersonRequestFactory();
			var person = new Person();
			var personRequest = personRequestFactory.CreatePersonRequest(person);
			var overTimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime), DateOnly.Today.ToDateTimePeriod(TimeZoneInfo.Utc));
			personRequest.Request = overTimeRequest;
			PersonRequestRepository.Add(personRequest);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}
	}
}
