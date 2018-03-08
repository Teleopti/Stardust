using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Staffing
{
	[InfrastructureTest]
	public class SendUpdateStaffingReadModelHandlerTest
	{
		public SendUpdateStaffingReadModelHandler Target;
		public IEventPublisherScope Publisher;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IScenarioRepository ScenarioRepository;
		public WithUnitOfWork WithUnitOfWork;

		[Ignore("I don't understand the infra test setup anymore")]
		[Test]
		public void ShouldSendUpdateJob()
		{
			WithUnitOfWork.Do(() =>
			{
				//logon blah blah
				var scenario = ScenarioFactory.CreateScenario("Default scenario", true, false);
				ScenarioRepository.Add(scenario);
			});

			var eventPublisher = new LegacyFakeEventPublisher();
			using (Publisher.OnThisThreadPublishTo(eventPublisher))
			{
				Target.Handle(new TenantMinuteTickEvent());
			}
			(eventPublisher.PublishedEvents.First() as UpdateStaffingLevelReadModelEvent).Should().Not.Be.Null();
		}
	}
}