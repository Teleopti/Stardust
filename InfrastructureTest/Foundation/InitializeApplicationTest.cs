using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Exceptions;
using IMessageSender = Teleopti.Ccc.Infrastructure.UnitOfWork.IMessageSender;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture, Category("LongRunning")]
	public class InitializeApplicationTest
	{
		private IDataSourcesFactory datasourcesFactory;
		private IMessageBroker messBroker;
		private MockRepository mocks;
		private IState stateStub;
		private InitializeApplication target;

		/// <summary>
		/// Runs once per test.
		/// </summary>
		[SetUp]
		public void Setup()
		{
			stateStub = new UsedInTestState();
			mocks = new MockRepository();
			messBroker = mocks.StrictMock<IMessageBroker>();
			datasourcesFactory = mocks.StrictMock<IDataSourcesFactory>();
			target = new InitializeApplication(datasourcesFactory, messBroker);
		}

		/// <summary>
		/// Runs at teardown for this test fixture
		/// </summary>
		[TearDown]
		public void Teardown()
		{
			clearDummyXml();
		}

		/// <summary>
		/// Runs once per test fixture
		/// </summary>
		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			var directory = Directory.GetCurrentDirectory();
			xmlFile1 = directory + @"\dummy1.nhib.xml";
			xmlFile2 = directory + @"\dummy2.nhib.xml";
		}

		
		[Test]
		public void TestStartWithHibernateConfigStrings()
		{
			var appSettingValue = DateTime.Now.ToLongTimeString();
			IDictionary<string, string> appSettings = new Dictionary<string, string> {
															  {"HelpUrl", appSettingValue},
															  {"MatrixWebSiteUrl", "http://localhost/Analytics"},
															  {"MessageBroker", "http://localhost/signalr"}
														  };

			ICollection<string> nhibConfigurations = new List<string>();

			var nhibString1 = XmlText("test", string.Empty);
			var nhibString2 = XmlText(string.Empty, correctMatrix());
			var nhibXml1 = XElement.Parse(nhibString1);
			var nhibXml2 = XElement.Parse(nhibString2);

			// note this wierd behaviour of NUnit, which is why we need to ignore XElement arguments later
			Assert.AreNotEqual(nhibXml1, XElement.Parse(nhibString1));

			nhibConfigurations.Add(nhibString1);
			nhibConfigurations.Add(nhibString2);

			var ds1 = mocks.StrictMock<IDataSource>();
			var ds2 = mocks.StrictMock<IDataSource>();
			var uowF1 = mocks.StrictMock<IUnitOfWorkFactory>();
			var uowF2 = mocks.StrictMock<IUnitOfWorkFactory>();

			using (mocks.Record())
			{
				Expect.Call(ds1.Application).Return(uowF1).Repeat.Any();
				Expect.Call(ds1.AuthenticationTypeOption).PropertyBehavior();
				Expect.Call(ds2.Application).Return(uowF2).Repeat.Any();
				Expect.Call(ds2.AuthenticationTypeOption).PropertyBehavior();
				Expect.Call(uowF1.Name).Return("dummy1").Repeat.Any();
				Expect.Call(uowF2.Name).Return("dummy2").Repeat.Any();
				Expect.Call(datasourcesFactory.TryCreate(nhibXml1, out ds1))
					.OutRef(ds1)
					.Return(true)
					.IgnoreArguments()
					.Constraints(
					Is.Matching<XElement>(x => x.ToString() == nhibXml1.ToString()), Is.Anything());
				Expect.Call(datasourcesFactory.TryCreate(nhibXml2, out ds2))
					.OutRef(ds2)
					.Return(true)
					.IgnoreArguments()
					.Constraints(
					Is.Matching<XElement>(x => x.ToString() == nhibXml2.ToString()), Is.Anything());
				messBroker.StartBrokerService();
				Expect.Call(messBroker.ServerUrl).PropertyBehavior();
			}

			using (new StateHolderModificationContext())
			{
				using (mocks.Playback())
				{
					ds1.AuthenticationTypeOption = AuthenticationTypeOption.Unknown;
					ds2.AuthenticationTypeOption = AuthenticationTypeOption.Unknown;
					target.Start(stateStub, appSettings, nhibConfigurations, null);
					Assert.AreEqual(2, StateHolder.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection.Count());
					CollectionAssert.Contains(StateHolder.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection, ds1);
					CollectionAssert.Contains(StateHolder.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection, ds2);

					Assert.AreEqual(StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["HelpUrl"], appSettingValue);
				}
				target.UnavailableDataSources.Any().Should().Be.False();
				Assert.AreSame(datasourcesFactory, target.DataSourcesFactory);
			}
		}

		[Test]
		public void ShouldHandleNotAvailableDataSources()
		{
			IDictionary<string, string> appSettings = new Dictionary<string, string>
			                                          	{
															  {"MessageBroker", "http://localhost/signalr"}
			                                          	};
			ICollection<string> nhibConfigurations = new List<string>();

			var nhibString1 = XmlText("dummy1", string.Empty);
			var nhibString2 = XmlText("dummy2", correctMatrix());
			var nhibXml1 = XElement.Parse(nhibString1);
			var nhibXml2 = XElement.Parse(nhibString2);

			nhibConfigurations.Add(nhibString1);
			nhibConfigurations.Add(nhibString2);

			var ds1 = mocks.StrictMock<IDataSource>();
			var uowF1 = mocks.StrictMock<IUnitOfWorkFactory>();

			using (mocks.Record())
			{
				Expect.Call(ds1.Application).Return(uowF1).Repeat.Any();
				Expect.Call(ds1.AuthenticationTypeOption).PropertyBehavior();
				Expect.Call(uowF1.Name).Return("dummy1").Repeat.Any();
				Expect.Call(datasourcesFactory.TryCreate(nhibXml1, out ds1))
					.OutRef(ds1)
					.Return(true)
					.IgnoreArguments()
					.Constraints(
					Is.Matching<XElement>(x => x.ToString() == nhibXml1.ToString()), Is.Anything());
				IDataSource ds2;
				Expect.Call(datasourcesFactory.TryCreate(nhibXml2, out ds2))
					.Return(false)
					.IgnoreArguments()
					.Constraints(
					Is.Matching<XElement>(x => x.ToString() == nhibXml2.ToString()), Is.Anything());
				messBroker.StartBrokerService();
				Expect.Call(messBroker.ServerUrl).PropertyBehavior();
			}

			using (new StateHolderModificationContext())
			{
				using (mocks.Playback())
				{
					ds1.AuthenticationTypeOption = AuthenticationTypeOption.Unknown;
					target.Start(stateStub, appSettings, nhibConfigurations, null);
					Assert.AreEqual(1, StateHolder.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection.Count());
					CollectionAssert.Contains(StateHolder.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection, ds1);
				}
				target.UnavailableDataSources.First().Should().Be.EqualTo("dummy2");
			}
		}

		[Test]
		public void VerifyDefaultProperty()
		{
			MessageBrokerContainer.Configure(null, null, null);
			target = new InitializeApplication(new DataSourcesFactory(null, new List<IMessageSender>(), DataSourceConfigurationSetter.ForTest()), 
				MessageBrokerContainer.CompositeClient());
		}


		// Private Methods (5) 

		private void clearDummyXml()
		{
			if (File.Exists(xmlFile1))
			{
				File.Delete(xmlFile1);
			}
			if (File.Exists(xmlFile2))
			{
				File.Delete(xmlFile2);
			}
		}

		private static string correctMatrix()
		{
			return @"<matrix name=""matrixName""><connectionString>matrixDatabase</connectionString></matrix>";
		}

		private void createDummyXml()
		{
			using (var file1 = new StreamWriter(xmlFile1))
			{
				file1.WriteLine(XmlText("test", string.Empty));
			}
			using (var file2 = new StreamWriter(xmlFile2))
			{
				file2.WriteLine(XmlText(string.Empty, correctMatrix()));
			}
		}

		private static IDictionary<string, string> nHibSettings()
		{
			IDictionary<string, string> ret = new Dictionary<string, string>();
			ret.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
			ret.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
			ret.Add("connection.connection_string", ConnectionStringHelper.ConnectionStringUsedInTests);
			ret.Add("show_sql", "false");
			ret.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");
			ret.Add("use_outer_join", "true");
			ret.Add("default_schema", "nhtest2.dbo");

			return ret;
		}

		private static string XmlText(string applicationDataSourceName, string matrixInfo)
		{
			return
				string.Concat(
				@"<?xml version=""1.0"" encoding=""utf-8"" ?>
				   <datasource>
					<authenticationType>", AuthenticationTypeOption.Application.ToString(), "</authenticationType>",
					@"<hibernate-configuration  xmlns=""urn:nhibernate-configuration-2.2"" >
					  <session-factory name=""",
				applicationDataSourceName,
				@""">
						<!-- properties -->
						<property name=""connection.provider"">NHibernate.Connection.DriverConnectionProvider</property>
						<property name=""connection.driver_class"">NHibernate.Driver.SqlClientDriver</property>
						<property name=""connection.connection_string"">",
				ConnectionStringHelper.ConnectionStringUsedInTests,
				@"</property>
						<property name=""show_sql"">false</property> 
						<property name=""dialect"">NHibernate.Dialect.MsSql2005Dialect</property>
						<property name=""use_outer_join"">true</property>
						<property name=""default_schema"">nhtest2.dbo</property>
					  </session-factory >
					</hibernate-configuration>
					",
				matrixInfo, @"
				  </datasource>
				");
		}

		private string xmlFile1, xmlFile2;

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