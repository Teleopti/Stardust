using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[ExtendScope(typeof(ScheduleChangedInDefaultScenarioNotificationNew))]
	[ExtendScope(typeof(ProjectionChangedEventPublisher))]
	[DomainTest]
	public class ScheduleChangedInDefaultScenarioTest
	{
		public IEventPublisher EventPublisher;
		public FakeMessageSender Sender;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		
		[Test]
		public void ShouldHandleScheduleChangedEvent()
		{
			var scenario = new Scenario{DefaultScenario = true}.WithId();
			ScenarioRepository.Has(scenario);				
			var agent = new Person().WithPersonPeriod().WithId();
			PersonRepository.Has(agent);
			
			EventPublisher.Publish(new ScheduleChangedEvent
			{
				LogOnDatasource = DomainTestAttribute.DefaultTenantName,
				LogOnBusinessUnitId = DomainTestAttribute.DefaultBusinessUnitId,
				ScenarioId = scenario.Id.Value,
				StartDateTime = DateTime.UtcNow,
				EndDateTime = DateTime.UtcNow.AddHours(3),
				PersonId = agent.Id.Value
			});    
      
			Sender.NotificationsOfDomainType<IScheduleChangedInDefaultScenario>()
				.Should().Have.Count.EqualTo(1);
		}
	}
}