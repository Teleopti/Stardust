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
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	public class AgentBadgeCalculationConsumerTest
	{
		private IDataSource _dataSource;
		private IUnitOfWorkFactory _uowFactory;
		private IRepositoryFactory _repositoryFactory;
		private IStatisticRepository _statisticsRepository;
		private IPersonRepository _personRepository;
		private IUnitOfWork _uow;
		private IStatelessUnitOfWork _statelessUow;
		private IGlobalSettingDataRepository _globalSettingDataRepository;
		private IAgentBadgeSettingsRepository _agentbadgeSettingsRepository;
		private IPerson _person;
		private AdherenceReportSetting _adherenceReportSetting;
		private IEnumerable<ISimpleTimeZone> _allTimezones;
		private DateTime _calculationDate;
		private ISimpleTimeZone timezone;
		private int timezoneId;
		private IPushMessageRepository _pushMessageRepository;

		[Test]
		public void ShouldSendNextCalculateMessageOneDayAfter()
		{
			var serviceBus = new ServiceBusMock();
			var target = new AgentBadgeCalculationConsumerForTest(serviceBus, _repositoryFactory, _dataSource);
			var message = new AgentBadgeCalculateMessage
			{
				IsInitialization = false,
				TimezoneId = timezoneId
			};

			target.Consume(message);

			var totalSecondsOfSentTime = (int)(serviceBus.DelaySentTime - DateTime.Now.Date).TotalSeconds;
			var tomorrowForTimezone = DateTime.UtcNow.Date.AddDays(1).AddMinutes(-timezone.Distance).ToLocalTime();

			var totalSecondsOfNow = (int)(tomorrowForTimezone - DateTime.Now.Date).TotalSeconds;
			totalSecondsOfSentTime.Should().Be.EqualTo(totalSecondsOfNow);

			var delaySendMessage = (AgentBadgeCalculateMessage)serviceBus.DelaySentMessage[0];
			delaySendMessage.IsInitialization.Should().Be.EqualTo(message.IsInitialization);
			delaySendMessage.TimezoneId.Should().Be.EqualTo(message.TimezoneId);
		}

		[SetUp]
		public void Setup()
		{
			_dataSource = MockRepository.GenerateStub<IDataSource>();
			_uowFactory = MockRepository.GenerateStub<IUnitOfWorkFactory>();
			_dataSource.Stub(x => x.Statistic).Return(_uowFactory);
			_dataSource.Stub(x => x.Application).Return(_uowFactory);

			_repositoryFactory = MockRepository.GenerateStub<IRepositoryFactory>();
			_statisticsRepository = MockRepository.GenerateStub<IStatisticRepository>();
			_personRepository = MockRepository.GenerateStub<IPersonRepository>();
			_globalSettingDataRepository = MockRepository.GenerateStub<IGlobalSettingDataRepository>();
			_agentbadgeSettingsRepository = MockRepository.GenerateStub<IAgentBadgeSettingsRepository>();
			_pushMessageRepository = MockRepository.GenerateStub<IPushMessageRepository>();

			_uow = MockRepository.GenerateStub<IUnitOfWork>();
			_uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_uow);
			_statelessUow = MockRepository.GenerateStub<IStatelessUnitOfWork>();
			_uowFactory.Stub(x => x.CreateAndOpenStatelessUnitOfWork()).Return(_statelessUow);

			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());
			_adherenceReportSetting = new AdherenceReportSetting();
			_allTimezones = new List<ISimpleTimeZone>
			{
				new SimpleTimeZone {Id = -1, Name = "UTC-1", Distance = -60},
				new SimpleTimeZone {Id = 0, Name = "UTC", Distance = 0},
				new SimpleTimeZone {Id = 1, Name = "UTC+1", Distance = 60},
				new SimpleTimeZone {Id = 8, Name = "China", Distance = 480},
				new SimpleTimeZone {Id = 10, Name = "UTC+10", Distance = 600}
			};
			timezone = _allTimezones.First();
			timezoneId = timezone.Id;

			_calculationDate = DateTime.UtcNow.AddMinutes(_allTimezones.First().Distance).Date.AddDays(-1);

			_repositoryFactory.Stub(x => x.CreateStatisticRepository()).Return(_statisticsRepository);
			_repositoryFactory.Stub(x => x.CreatePersonRepository(_uow)).Return(_personRepository);
			_repositoryFactory.Stub(x => x.CreateGlobalSettingDataRepository(_uow)).Return(_globalSettingDataRepository);
			_repositoryFactory.Stub(x => x.CreateAgentBadgeSettingsRepository(_uow)).Return(_agentbadgeSettingsRepository);
			_repositoryFactory.Stub(x => x.CreatePushMessageRepository(_uow)).Return(_pushMessageRepository);

			_statisticsRepository.Stub(x => x.LoadAllTimeZones(_statelessUow)).Return(_allTimezones);
			_agentbadgeSettingsRepository.Stub(x => x.LoadAll()).Return(new List<IAgentBadgeThresholdSettings>
			{
				new AgentBadgeThresholdSettings
				{
					EnableBadge = true,
					SilverToBronzeBadgeRate = 5,
					GoldToSilverBadgeRate = 2
				}
			});

			_globalSettingDataRepository.Stub(
				x => x.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting())).IgnoreArguments()
				.Return(_adherenceReportSetting);
			_personRepository.Stub(x => x.LoadAll()).Return(new List<IPerson> { _person });
		}

		[Test]
		public void ShouldAwardBronzeForAnsweredCalls()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAnsweredCalls(_statelessUow, timezoneId, _calculationDate))
				.Return(new List<Guid> {_person.Id.Value});

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage
			{
				IsInitialization = false,
				TimezoneId = timezoneId
			});

			_person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls).BronzeBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAwardSilverForAnsweredCalls()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAnsweredCalls(_statelessUow, timezoneId, _calculationDate))
				.Return(new List<Guid> {_person.Id.Value});
			_person.AddBadge(new Domain.Common.AgentBadge(){BadgeType = BadgeType.AnsweredCalls, BronzeBadge = 4});

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage
			{
				IsInitialization = false,
				TimezoneId = timezoneId
			});

			_person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls).BronzeBadge.Should().Be.EqualTo(0);
			_person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls).SilverBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAwardGoldForAnsweredCalls()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAnsweredCalls(_statelessUow, timezoneId, _calculationDate))
				.Return(new List<Guid> {_person.Id.Value});
			_person.AddBadge(new Domain.Common.AgentBadge(){BadgeType = BadgeType.AnsweredCalls, BronzeBadge = 4, SilverBadge = 1});

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage
			{
				IsInitialization = false,
				TimezoneId = timezoneId
			});

			_person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls).BronzeBadge.Should().Be.EqualTo(0);
			_person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls).SilverBadge.Should().Be.EqualTo(0);
			_person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls).GoldBadge.Should().Be.EqualTo(1);
		}
	
		[Test]
		public void ShouldAwardBronzeForAdherence()
		{
			_statisticsRepository.Stub(
				x => x.LoadAgentsOverThresholdForAdherence(_statelessUow, _adherenceReportSetting.CalculationMethod, timezoneId, _calculationDate))
				.Return(new List<Guid> {_person.Id.Value});

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage
			{
				IsInitialization = false,
				TimezoneId = timezoneId
			});

			_person.Badges.Single(x => x.BadgeType == BadgeType.Adherence).BronzeBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAwardBronzeForAHT()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsUnderThresholdForAHT(_statelessUow, timezoneId, _calculationDate)).Return(new List<Guid> { _person.Id.Value });
			
			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage
			{
				IsInitialization = false,
				TimezoneId = timezoneId
			});

			_person.Badges.Single(x => x.BadgeType == BadgeType.AverageHandlingTime).BronzeBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAwardBronzeForBothAdherenceAndAnsweredCalls()
		{
			_statisticsRepository.Stub(
				x => x.LoadAgentsOverThresholdForAdherence(_statelessUow, _adherenceReportSetting.CalculationMethod, timezoneId, _calculationDate))
				.Return(new List<Guid> {_person.Id.Value});
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAnsweredCalls(_statelessUow, timezoneId, _calculationDate))
				.Return(new List<Guid> {_person.Id.Value});

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage
			{
				IsInitialization = false,
				TimezoneId = timezoneId
			});

			_person.Badges.Single(x => x.BadgeType == BadgeType.Adherence).BronzeBadge.Should().Be.EqualTo(1);
			_person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls).BronzeBadge.Should().Be.EqualTo(1);
		}
	}

	public class AgentBadgeCalculationConsumerForTest : AgentBadgeCalculationConsumer
	{
		private readonly IDataSource _dataSource;

		public AgentBadgeCalculationConsumerForTest(IServiceBus serviceBus, IRepositoryFactory repositoryFactory,
			IDataSource dataSource)
			: base(serviceBus, repositoryFactory)
		{
			_dataSource = dataSource;
		}

		protected override IEnumerable<IDataSource> GetValidDataSources()
		{
			return new[] { _dataSource };
		}
	}

	public class ServiceBusMock : IServiceBus
	{
		public void Publish(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Notify(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Reply(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Send(Endpoint endpoint, params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Send(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void ConsumeMessages(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public IDisposable AddInstanceSubscription(IMessageConsumer consumer)
		{
			throw new NotImplementedException();
		}

		public void Subscribe<T>()
		{
			throw new NotImplementedException();
		}

		public void Subscribe(Type type)
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe<T>()
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe(Type type)
		{
			throw new NotImplementedException();
		}

		public void DelaySend(Endpoint endpoint, DateTime time, params object[] msgs)
		{
			throw new NotImplementedException();
		}

		public void DelaySend(DateTime time, params object[] msgs)
		{
			DelaySentTime = time;
			DelaySentMessage = msgs;
		}

		public object[] DelaySentMessage { get; set; }

		public DateTime DelaySentTime { get; set; }

		public Endpoint Endpoint { get; private set; }
		public CurrentMessageInformation CurrentMessageInformation { get; private set; }

		// Ignore compile warning "The event 'Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge.ServiceBusMock.ReroutedEndpoint' is never used".
		// This warning will be treated as error and cause build on server failed.
#pragma warning disable 0067
		public event Action<Reroute> ReroutedEndpoint;
#pragma warning restore 0067
	}
}
