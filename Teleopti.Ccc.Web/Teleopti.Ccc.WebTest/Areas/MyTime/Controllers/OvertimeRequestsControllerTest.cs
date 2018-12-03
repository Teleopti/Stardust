using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	[RequestsTest]
	public class OvertimeRequestsControllerTest : IIsolateSystem
	{
		public OvertimeRequestsController OvertimeRequestsController;
		public ICurrentScenario CurrentScenario;
		public FakePersonAssignmentRepository FakeAssignmentRepository;
		public FakeActivityRepository FakeActivityRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeLoggedOnUser FakeLoggedOnUser;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("default") { DefaultScenario = true }))
				.For<IScenarioRepository>();
			isolate.UseTestDouble(new MutableNow(new DateTime(2018, 01, 08, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
		}

		[Test]
		public void ShouldReturnDefaultStartTimeString()
		{
			Now.Is(new DateTime(2018, 1, 8, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(2018, 1, 10);
			var agent = PersonFactory.CreatePersonWithGuid("agent", "one");
			var timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			agent.PermissionInformation.SetDefaultTimeZone(timezone);

			FakeLoggedOnUser.SetFakeLoggedOnUser(agent);
			var phone = ActivityFactory.CreateActivity("phone activity");
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(agent, CurrentScenario.Current(), date);
			personAssignment.AddActivity(phone, new DateTimePeriod(2018, 1, 10, 08, 2018, 1, 10, 17));
			FakeAssignmentRepository.Has(personAssignment);

			var result = OvertimeRequestsController.GetDefaultStartTime(date);
			var data = result.Data as OvertimeDefaultStartTimeResult;

			data.DefaultStartTimeString.Should().Be("2018-01-10 18:00");
		}

	}
}