using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
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
		private ITeamGamificationSettingRepository badgeSettingRep;
		private IServiceBus serviceBus;
		private BadgeCalculationInitConsumer target;
		private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
		private IUnitOfWorkFactory loggedOnUnitOfWorkFactory;
		private IToggleManager toggleManager;

		[SetUp]
		public void Setup()
		{
			businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			badgeSettingRep = MockRepository.GenerateMock<ITeamGamificationSettingRepository>();
			serviceBus = MockRepository.GenerateMock<IServiceBus>();
			currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			toggleManager = MockRepository.GenerateMock<IToggleManager>();
			target = new BadgeCalculationInitConsumer(serviceBus, badgeSettingRep, businessUnitRepository, currentUnitOfWorkFactory,toggleManager);
		}

		[Test]
		public void ShouldSendCalculateTimeZoneMessageWhenAgentBadgeEnable()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var message = new BadgeCalculationInitMessage
			{
				Timestamp = DateTime.Now,
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			var timezoneList = new List<TimeZoneInfo> {TimeZoneInfo.Local};

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(loggedOnUnitOfWorkFactory);
			badgeSettingRep.Stub(x => x.FindTeamGamificationSettingsByTeam(new Team())).IgnoreArguments()
				.Return(
					new TeamGamificationSetting
					{
						GamificationSetting = new GamificationSetting("TestSetting")
					});
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
		public void ShouldResendCalculateInitMessageWhenAgentBadgeDisabledForAllTeam()
		{
			var bussinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU");
			bussinessUnit.SetId(Guid.NewGuid());

			var now = DateTime.Now;
			var message = new BadgeCalculationInitMessage
			{
				Timestamp = now,
				LogOnBusinessUnitId = bussinessUnit.Id.GetValueOrDefault()
			};
			var timezoneList = new List<TimeZoneInfo> { TimeZoneInfo.Local };

			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(loggedOnUnitOfWorkFactory);
			badgeSettingRep.Stub(x => x.FindAllTeamGamificationSettingsSortedByTeam()).Return(
				new List<TeamGamificationSetting>());
			toggleManager.Stub(x => x.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)).Return(true);

			businessUnitRepository.Stub(x => x.LoadAllTimeZones()).Return(timezoneList);

			target.Consume(message);

			serviceBus.AssertWasCalled(x => x.DelaySend(DateTime.Now, new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<DateTime>(m => m == now.Date.AddDays(1))),
						Rhino.Mocks.Constraints.Is.Matching(
							new Predicate<object[]>(m => ((BadgeCalculationInitMessage) m[0]).Identity == message.Identity))));
		}
	}
}
