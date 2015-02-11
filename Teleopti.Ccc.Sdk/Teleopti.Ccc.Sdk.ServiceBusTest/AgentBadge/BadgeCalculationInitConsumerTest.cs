using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class BadgeCalculationInitConsumerTest
	{
		private IBusinessUnitRepository businessUnitRepository;
		private IAgentBadgeSettingsRepository badgeSettingRep;
		private IServiceBus serviceBus;
		private BadgeCalculationInitConsumer target;
		private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
		private IUnitOfWorkFactory loggedOnUnitOfWorkFactory;
		private IToggleManager toggleManager;

		[SetUp]
		public void Setup()
		{
			businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			badgeSettingRep = MockRepository.GenerateMock<IAgentBadgeSettingsRepository>();
			serviceBus = MockRepository.GenerateMock<IServiceBus>();
			currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			toggleManager = MockRepository.GenerateMock<IToggleManager>();
			target = new BadgeCalculationInitConsumer(serviceBus, badgeSettingRep, businessUnitRepository, currentUnitOfWorkFactory,toggleManager);
		}

		[Test]
		public void ShouldSendCalculateTimeZoneMessageWhen31318FeatureCloseAndAgentBadgeEnable()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new BadgeCalculationInitMessage();
			message.Timestamp = DateTime.Now;
			message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
			var timezoneList = new List<TimeZoneInfo>{TimeZoneInfo.Local};

			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(loggedOnUnitOfWorkFactory);
			badgeSettingRep.Stub(x => x.GetSettings()).Return(new AgentBadgeSettings() { BadgeEnabled = true });
			toggleManager.Stub(x => x.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(false);

			businessUnitRepository.Stub(x => x.LoadAllTimeZones()).Return(timezoneList);

			target.Consume(message);

			serviceBus.AssertWasCalled(x => x.Send(new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(
							new Predicate<object[]>(m => ((CalculateTimeZoneMessage) m[0]).TimeZoneCode == TimeZoneInfo.Local.Id))));
		}

		[Test]
		public void ShouldSendCalculateTimeZoneMessageWhen31318FeatrueOpenAndAgentBadgeDisable()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new BadgeCalculationInitMessage();
			message.Timestamp = DateTime.Now;
			message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
			var timezoneList = new List<TimeZoneInfo> { TimeZoneInfo.Local };

			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(loggedOnUnitOfWorkFactory);
			badgeSettingRep.Stub(x => x.GetSettings()).Return(new AgentBadgeSettings() { BadgeEnabled = false });
			toggleManager.Stub(x => x.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(true);

			businessUnitRepository.Stub(x => x.LoadAllTimeZones()).Return(timezoneList);

			target.Consume(message);

			serviceBus.AssertWasCalled(x => x.Send(new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(
							new Predicate<object[]>(m => ((CalculateTimeZoneMessage)m[0]).TimeZoneCode == TimeZoneInfo.Local.Id))));
		}
	}
}
