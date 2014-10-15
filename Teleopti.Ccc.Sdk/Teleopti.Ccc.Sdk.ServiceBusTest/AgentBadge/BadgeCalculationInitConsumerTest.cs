﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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

		[SetUp]
		public void Setup()
		{
			businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			badgeSettingRep = MockRepository.GenerateMock<IAgentBadgeSettingsRepository>();
			serviceBus = MockRepository.GenerateMock<IServiceBus>();
			currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			target = new BadgeCalculationInitConsumer(serviceBus, badgeSettingRep, businessUnitRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldSendCalculateTimeZoneMessage()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new BadgeCalculationInitMessage();
			message.Timestamp = DateTime.Now;
			message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
			var timezoneList = new List<TimeZoneInfo>{TimeZoneInfo.Local};

			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(loggedOnUnitOfWorkFactory);
			badgeSettingRep.Stub(x => x.GetSettings()).Return(new AgentBadgeThresholdSettings() { BadgeEnabled = true });
			businessUnitRepository.Stub(x => x.LoadAllTimeZones()).Return(timezoneList);

			target.Consume(message);

			serviceBus.AssertWasCalled(x => x.Send(new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(
							new Predicate<object[]>(m => ((CalculateTimeZoneMessage) m[0]).TimeZoneCode == TimeZoneInfo.Local.Id))));
		}
	}
}
