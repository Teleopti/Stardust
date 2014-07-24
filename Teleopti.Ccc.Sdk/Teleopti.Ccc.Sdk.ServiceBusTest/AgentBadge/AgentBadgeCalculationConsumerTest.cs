using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Impl;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.Messages;
using SharpTestsEx;
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
		private IGlobalSettingDataRepository _globalSettingDataRepository;
		private IPerson _person;
		private AdherenceReportSetting _adherenceReportSetting;

		[Test]
		public void ShouldSendNextCalculateMessageOneDayAfter()
		{
			var serviceBus = new ServiceBusMock();
			var target = new AgentBadgeCalculationConsumer(serviceBus, null);
			var message = new AgentBadgeCalculateMessage();

			target.Consume(message);

			var totalSecondsOfSentTime = (int)(serviceBus.DelaySentTime - DateTime.Now.Date).TotalSeconds;
			var totalSecondsOfNow = (int)(DateTime.Now.AddDays(1) - DateTime.Now.Date).TotalSeconds;
			totalSecondsOfSentTime.Should().Be.EqualTo(totalSecondsOfNow);
			serviceBus.DelaySentMessage[0].Should().Be.EqualTo(message);
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

			_uow = MockRepository.GenerateStub<IUnitOfWork>();
			_uowFactory.Stub(x => x.CurrentUnitOfWork()).Return(_uow);

			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());
			_adherenceReportSetting = new AdherenceReportSetting();

			_repositoryFactory.Stub(x => x.CreateStatisticRepository()).Return(_statisticsRepository);
			_repositoryFactory.Stub(x => x.CreatePersonRepository(_uow)).Return(_personRepository);
			_repositoryFactory.Stub(x => x.CreateGlobalSettingDataRepository(_uow)).Return(_globalSettingDataRepository);

			_globalSettingDataRepository.Stub(
				x => x.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting())).IgnoreArguments()
				.Return(_adherenceReportSetting);
			_personRepository.Stub(x => x.LoadAll()).Return(new List<IPerson>() { _person });
		}

		[Test]
		public void ShouldAwardBronzeForAnsweredCalls()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAnsweredCalls(_uow, DateTime.Now)).Return(new List<Guid>{_person.Id.Value});

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage());

			_person.Badges.BronzeBadge.Should().Be.EqualTo(1);
		}

	
		[Test]
		public void ShouldAwardBronzeForAdherence()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAdherence(_uow, _adherenceReportSetting.CalculationMethod, DateTime.Now)).Return(new List<Guid> { _person.Id.Value });

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage());

			_person.Badges.BronzeBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAwardBronzeForAHT()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsUnderThresholdForAHT(_uow, DateTime.Now)).Return(new List<Guid> { _person.Id.Value });
			
			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage());

			_person.Badges.BronzeBadge.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAwardBronzeForBothAdherenceAndAnsweredCalls()
		{
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAdherence(_uow, _adherenceReportSetting.CalculationMethod, DateTime.Now)).Return(new List<Guid> { _person.Id.Value });
			_statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAnsweredCalls(_uow, DateTime.Now)).Return(new List<Guid> { _person.Id.Value });

			var target = new AgentBadgeCalculationConsumerForTest(null, _repositoryFactory, _dataSource);
			target.Consume(new AgentBadgeCalculateMessage());

			_person.Badges.BronzeBadge.Should().Be.EqualTo(2);
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

		protected override IEnumerable<IDataSource> GetRegisteredDataSourceCollection()
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
