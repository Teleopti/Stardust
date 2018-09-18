using System.IO;
using System.Threading;
using Autofac;
using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Messaging.Client;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[SetUpFixture]
	public class NUnitSetup
	{
		[OneTimeSetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			XmlConfigurator.Configure();

			DataSourceHelper.CreateDatabases();

			TestSiteConfigurationSetup.Setup();

			IntegrationIoCTest.Setup(builder =>
			{
				builder.RegisterType<TestConfiguration>().SingleInstance();
				builder.RegisterType<DataCreator>().SingleInstance().ApplyAspects();
				builder.RegisterType<StatesSender>().SingleInstance().ApplyAspects();
				builder.RegisterType<ScheduleInvalidator>().SingleInstance().ApplyAspects();
				builder.RegisterType<FakeEventPublisher>().SingleInstance();
				builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
				builder.RegisterType<SynchronzieWaiter>().SingleInstance().ApplyAspects();
			}, arguments => { arguments.AllEventPublishingsAsSync = true; }, this);

			//TestSiteConfigurationSetup.TearDown();
		}
	}

	public class SynchronzieWaiter
	{
		private readonly WithUnitOfWork _unitOfWork;
		private readonly WithReadModelUnitOfWork _readModelUnitOfWork;
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly IRtaEventStoreTestReader _events;

		public SynchronzieWaiter(WithUnitOfWork unitOfWork, WithReadModelUnitOfWork readModelUnitOfWork, IKeyValueStorePersister keyValueStore, IRtaEventStoreTestReader events)
		{
			_unitOfWork = unitOfWork;
			_readModelUnitOfWork = readModelUnitOfWork;
			_keyValueStore = keyValueStore;
			_events = events;
		}

		[TestLog]
		public virtual void WaitForSyncronize()
		{
			while (true)
			{
				if (GetLatestStoredRtaEventId() == GetLatestSynchronizedRtaEventId())
					break;
				Thread.Sleep(100);
			}
		}

		[TestLog]
		protected virtual int GetLatestSynchronizedRtaEventId()
		{
			return _readModelUnitOfWork.Get(() => _keyValueStore.Get("LatestSynchronizedRTAEvent", 0));
		}

		[TestLog]
		protected virtual int GetLatestStoredRtaEventId()
		{
			return _unitOfWork.Get(() => _events.LoadLastIdForTest());
		}
	}
}