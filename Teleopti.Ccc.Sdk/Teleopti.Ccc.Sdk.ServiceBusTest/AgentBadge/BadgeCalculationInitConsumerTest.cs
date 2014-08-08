using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Impl;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.Messages;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	class BadgeCalculationInitConsumerTest
	{
		private MockRepository mocks;
		private IBusinessUnitRepository businessUnitRepository;
		private IAgentBadgeSettingsRepository badgeSettingRep;
		private IServiceBus serviceBus;
		private BadgeCalculationInitConsumer target;
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			businessUnitRepository = mocks.DynamicMock<IBusinessUnitRepository>();
			badgeSettingRep = mocks.DynamicMock<IAgentBadgeSettingsRepository>();
			serviceBus = mocks.DynamicMock<IServiceBus>();
			target = new BadgeCalculationInitConsumer(serviceBus, badgeSettingRep, businessUnitRepository);
		}

		[Test]
		public void IsConsumerCalled()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new StartUpBusinessUnit();
			message.Timestamp = DateTime.Now;
			message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
			var timezoneList = new List<TimeZoneInfo>{TimeZoneInfo.Local};

			Expect.Call(badgeSettingRep.LoadAll()).Return(new List<IAgentBadgeThresholdSettings>{new AgentBadgeThresholdSettings(){EnableBadge = true}});
			Expect.Call(businessUnitRepository.LoadAllTimeZones()).Return(timezoneList);

			mocks.ReplayAll();
			target.Consume(message);
			mocks.VerifyAll();

		}
	}
}
