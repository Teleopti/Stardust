using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	class BadgeCalculationInitConsumerTest
	{
		private IBusinessUnitRepository businessUnitRepository;
		private IAgentBadgeSettingsRepository badgeSettingRep;
		private IServiceBus serviceBus;
		private BadgeCalculationInitConsumer target;
		[SetUp]
		public void Setup()
		{
			businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			badgeSettingRep = MockRepository.GenerateMock<IAgentBadgeSettingsRepository>();
			serviceBus = MockRepository.GenerateMock<IServiceBus>();
			target = new BadgeCalculationInitConsumer(serviceBus, badgeSettingRep, businessUnitRepository);
		}

		[Test]
		public void ShouldSendCalculateTimeZoneMessage()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new StartUpBusinessUnit();
			message.Timestamp = DateTime.Now;
			message.BusinessUnitId = bussinessUnit.Id.GetValueOrDefault();
			var timezoneList = new List<TimeZoneInfo>{TimeZoneInfo.Local};
			
			badgeSettingRep.Stub(x =>  x.LoadAll()).Return(new List<IAgentBadgeThresholdSettings>{new AgentBadgeThresholdSettings(){EnableBadge = true}});
			businessUnitRepository.Stub(x => x.LoadAllTimeZones()).Return(timezoneList);

			target.Consume(message);

			serviceBus.AssertWasCalled(x => x.Send(new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(
							new Predicate<object[]>(m => ((CalculateTimeZoneMessage) m[0]).TimeZone == TimeZoneInfo.Local))));
		}
	}
}
