using System;
using System.Linq;
using Autofac;
using Autofac.Core.Registration;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[RemoveMeWithToggle("rename", Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	[DomainTest]
	[DefaultData]
	[Toggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class ScheduleChangedInDefaultScenarioNotificationNewTest
	{
		public ScheduleChangedInDefaultScenarioNotificationNew Target;
		public FakeMessageSender Sender;
		public FakeScenarioRepository ScenarioRepository;

		public IComponentContext TempContainer;
		[Test]
		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		public void MustNotUseNewHandler()
		{
			Assert.Throws<ComponentNotRegisteredException>(() =>
				TempContainer.Resolve<ScheduleChangedInDefaultScenarioNotification>());
		}

		
		[Test]
		public void ShouldSendMessageInDefaultScenario()
		{
			var scenario = new Scenario {DefaultScenario = true}.WithId();
			ScenarioRepository.Has(scenario);
			var personId = Guid.NewGuid();

			Target.Handle(new ScheduleChangedEventForTest
			{
				ScenarioId = scenario.Id.Value,
				PersonId = personId
			});

			Sender.NotificationsOfDomainType<IScheduleChangedInDefaultScenario>()
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendMessageInNonDefaultScenario()
		{
			var scenario = new Scenario {DefaultScenario = false}.WithId();
			ScenarioRepository.Has(scenario);
			var person = Guid.NewGuid();

			Target.Handle(new ScheduleChangedEventForTest
			{
				ScenarioId = scenario.Id.Value,
				PersonId = person
			});

			Sender.NotificationsOfDomainType<IScheduleChangedInDefaultScenario>()
				.Should().Have.Count.EqualTo(0);
		}
	}
}