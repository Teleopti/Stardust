using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	[ExtendScope(typeof(ScheduleReadModelWrapperHandler))]
	[ExtendScope(typeof(ProjectionChangedEventPublisherNew))]
	public class PersonScheduleDayReadModelUpdaterPublishingTest : ISetup
	{
		public IEventPublisher EventPublisher;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonScheduleDayReadModelPersister PersonScheduleDayReadModelPersister;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public IConfigReader Foo;
		
		[Test]
		public void ShouldHandleScheduleChangedEvent()
		{
			var scenario = new Scenario{DefaultScenario = true}.WithId();
			ScenarioRepository.Has(scenario);				
			var agent = new Person().WithPersonPeriod().WithId();
			PersonRepository.Has(agent);
			BusinessUnitRepository.Has(new Domain.Common.BusinessUnit("_").WithId(DomainTestAttribute.DefaultBusinessUnitId));
			
			EventPublisher.Publish(new ScheduleChangedEvent
			{
				LogOnDatasource = DomainTestAttribute.DefaultTenantName,
				LogOnBusinessUnitId = DomainTestAttribute.DefaultBusinessUnitId,
				ScenarioId = scenario.Id.Value,
				StartDateTime = DateTime.UtcNow,
				EndDateTime = DateTime.UtcNow.AddHours(3),
				PersonId = agent.Id.Value
			});

			PersonScheduleDayReadModelPersister.NumberOfUpdatedCalls
				.Should().Be.EqualTo(1);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonScheduleDayReadModelPersister>().For<IPersonScheduleDayReadModelPersister>();
			system.UseTestDouble<scheduleChangesSubscriptionPublisherDummy>().For<ScheduleChangesSubscriptionPublisher>();
			system.UseTestDouble<FakeScheduleDayReadModelRepository>().For<IScheduleDayReadModelRepository>();
		}

		private class scheduleChangesSubscriptionPublisherDummy : ScheduleChangesSubscriptionPublisher
		{
			public scheduleChangesSubscriptionPublisherDummy(IHttpServer server, INow now, IGlobalSettingDataRepository settingsRepository) : base(server, now, settingsRepository, null)
			{
			}
		}
	}
}