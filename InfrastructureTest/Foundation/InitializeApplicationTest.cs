using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Client;
using IMessageSender = Teleopti.Ccc.Infrastructure.UnitOfWork.IMessageSender;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture, Category("LongRunning")]
	public class InitializeApplicationTest
	{
		private IDataSourcesFactory datasourcesFactory;
		private IMessageBrokerComposite messBroker;
		private MockRepository mocks;
		private IState stateStub;
		private InitializeApplication target;

		[SetUp]
		public void Setup()
		{
			stateStub = new UsedInTestState();
			mocks = new MockRepository();
			messBroker = mocks.StrictMock<IMessageBrokerComposite>();
			datasourcesFactory = mocks.StrictMock<IDataSourcesFactory>();
			target = new InitializeApplication(datasourcesFactory, messBroker);
		}

		[Test]
		public void TestStartWithDataSource()
		{
			var appSettingValue = DateTime.Now.ToLongTimeString();
			IDictionary<string, string> appSettings = new Dictionary<string, string> {
															  {"HelpUrl", appSettingValue},
															  {"MatrixWebSiteUrl", "http://localhost/Analytics"},
															  {"MessageBroker", "http://localhost/signalr"}
														  };

			var ds1 = MockRepository.GenerateMock<IDataSource>();

			messBroker.StartBrokerService();
			Expect.Call(messBroker.ServerUrl).PropertyBehavior();


			using (new StateHolderModificationContext())
			{
				target.Start(stateStub, appSettings, ds1, null);

				Assert.AreEqual(StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpUrl"], appSettingValue);

				Assert.AreSame(datasourcesFactory, target.DataSourcesFactory);
			}
		}

		[Test]
		public void VerifyDefaultProperty()
		{
			MessageBrokerContainerDontUse.Configure(null, null, null);
			target = new InitializeApplication(new DataSourcesFactory(null, new List<IMessageSender>(), DataSourceConfigurationSetter.ForTest(), new CurrentHttpContext(), null),
				MessageBrokerContainerDontUse.CompositeClient());
		}

		private class StateHolderModificationContext : IDisposable
		{
			private readonly IStateReader _stateBefore;

			public StateHolderModificationContext()
			{
				_stateBefore = StateHolder.Instance.StateReader;
				StateHolderProxyHelper.ClearStateHolder();
			}

			public void Dispose()
			{
				StateHolderProxyHelper.ClearAndSetStateHolder((IState)_stateBefore);
			}
		}
	}
}