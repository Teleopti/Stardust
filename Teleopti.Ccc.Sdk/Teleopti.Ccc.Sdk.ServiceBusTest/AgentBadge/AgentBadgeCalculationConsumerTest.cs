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
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	public class AgentBadgeCalculationConsumerTest
	{
		[Test]
		public void ShouldSendNesCalculateMessageOneDayAfter()
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


		[Test]
		public void ShouldGiveAgentBronzeForAnsweredCalls()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());


			var dataSource = MockRepository.GenerateStub<IDataSource>();
			var uowFactory = MockRepository.GenerateStub<IUnitOfWorkFactory>();
			dataSource.Stub(x => x.Statistic).Return(uowFactory);
			dataSource.Stub(x => x.Application).Return(uowFactory);
			var uow = MockRepository.GenerateStub<IUnitOfWork>();
			uowFactory.Stub(x => x.CurrentUnitOfWork()).Return(uow);

			var repositoryFactory = MockRepository.GenerateStub<IRepositoryFactory>();
			var statisticsRepository = MockRepository.GenerateStub<IStatisticRepository>();
			var personRepository = MockRepository.GenerateStub<IPersonRepository>();
			repositoryFactory.Stub(x => x.CreateStatisticRepository()).Return(statisticsRepository);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);

			statisticsRepository.Stub(x => x.LoadAgentsOverThresholdForAnsweredCalls(uow)).Return(new List<Tuple<Guid, int>> { new Tuple<Guid, int>(person.Id.Value, 50) });
			personRepository.Stub(x => x.LoadAll()).Return(new List<IPerson>() { person });

			var target = new AgentBadgeCalculationConsumerForTest(null, repositoryFactory, dataSource);


			target.Consume(new AgentBadgeCalculateMessage());

			person.Badges.BronzeBadge.Should().Be.EqualTo(1);
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
		public event Action<Reroute> ReroutedEndpoint;
	}
}
