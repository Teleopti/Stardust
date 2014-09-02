using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class CalculateBadgeConsumerTest
	{
		private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private IServiceBus serviceBus;
		private IAgentBadgeSettingsRepository badgeSettingsRepository;
		private IStatisticRepository statisticRepository;
		private IAgentBadgeRepository badgeRepository;
		private IPersonRepository personRepository;
		private IGlobalSettingDataRepository globalSettingRepository;
		private IPushMessagePersister msgRepository;
		private CalculateBadgeConsumer target;
		private IAgentBadgeCalculator calculator;
		private INow now;
		private IUnitOfWorkFactory loggedOnUnitOfWorkFactory;
		private IUnitOfWork unitOfWork;

		[SetUp]
		public void Setup()
		{
			unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(loggedOnUnitOfWorkFactory);
			loggedOnUnitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			serviceBus = MockRepository.GenerateMock<IServiceBus>();
			badgeSettingsRepository = MockRepository.GenerateMock<IAgentBadgeSettingsRepository>();
			badgeSettingsRepository.Stub(x => x.LoadAll())
				.Return(new List<IAgentBadgeThresholdSettings> {new AgentBadgeThresholdSettings {EnableBadge = true}});

			statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			badgeRepository = MockRepository.GenerateMock<IAgentBadgeRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(
				x => x.FindPeopleInOrganization(new DateOnlyPeriod(new DateOnly(2014, 8, 7), new DateOnly(2014, 8, 9)), false))
				.Return(new List<IPerson> {new Person()});

			globalSettingRepository = MockRepository.GenerateMock<IGlobalSettingDataRepository>();
			globalSettingRepository.Stub(x => x.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting()))
				.IgnoreArguments()
				.Return(new AdherenceReportSetting());

			msgRepository = MockRepository.GenerateMock<IPushMessagePersister>();
			now = MockRepository.GenerateMock<INow>();
			calculator = new AgentBadgeCalculator(statisticRepository, badgeRepository);
			target = new CalculateBadgeConsumer(serviceBus, badgeSettingsRepository, personRepository, globalSettingRepository,
				msgRepository, unitOfWorkFactory, calculator, now);
		}
		[Test]
		public void ShouldSendCalculateBadgeMessageAtRightTime()
		{
			var timezone = TimeZoneInfo.Utc;
			var today = new DateTime(2014, 8, 8);
			var tomorrowUnspecified = new DateTime(2014, 8, 9, 0, 0, 0, DateTimeKind.Unspecified);
			var expectedNextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrowUnspecified.AddHours(5), timezone, TimeZoneInfo.Local);
			
			now.Stub(x => x.UtcDateTime()).Return(today);
			var calculationDate = TimeZoneInfo.ConvertTime(now.LocalDateOnly().AddDays(-1), TimeZoneInfo.Local, timezone);
			var message = new CalculateBadgeMessage
			{
				TimeZoneCode = timezone.Id,
				CalculationDate = new DateOnly(calculationDate)
			};
		
			target.Consume(message);

			serviceBus.AssertWasCalled(x => x.DelaySend(new DateTime(), new object()),
				o =>
					o.Constraints(
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<DateTime>(m => m == expectedNextMessageShouldBeProcessed)),
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<object[]>(m =>
						{
							var msg = ((CalculateBadgeMessage)m[0]);
							return msg.TimeZoneCode == TimeZoneInfo.Utc.Id
								&& msg.CalculationDate == message.CalculationDate.AddDays(1);
						}))));
		}
	}
}
