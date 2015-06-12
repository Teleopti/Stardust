#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

#endregion

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	/// <summary>
    /// Tests for ApplicationData
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class ApplicationDataTest
    {
        private MockRepository mocks;
        private IDictionary<string, string> _receivedSettings;
        private ReadOnlyCollection<IDataSource> dataSources;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            IList<IDataSource> dataSourceList = new List<IDataSource>();
            dataSourceList.Add(new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("test"), null, null));
            dataSources = new ReadOnlyCollection<IDataSource>(dataSourceList);

            _receivedSettings = new Dictionary<string, string>
                                    {
                                        {"HelpUrl", "http://wiki/ccc/"},
                                        {"MatrixWebSiteUrl", "http://localhost/Analytics"}
                                    };
        }

		[Test]
		public void MakeSureDataSourceExistsShouldAddIfNotExist()
		{
			var dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
			var dataSourceName = RandomName.Make();
			var appNhibConf = new Dictionary<string, string>();
			var statisticConnString = RandomName.Make();
			var dataSource = new FakeDataSource{DataSourceName = dataSourceName};
			dataSourcesFactory.Expect(x => x.Create(appNhibConf, statisticConnString)).Return(dataSource);
			var target = new ApplicationData(new Dictionary<string, string>(), Enumerable.Empty<IDataSource>(), null, null, dataSourcesFactory);
			target.Tenant(dataSourceName).Should().Be.EqualTo(null);
			target.MakeSureDataSourceExists(dataSourceName, RandomName.Make(), statisticConnString, appNhibConf);
			target.Tenant(dataSourceName).Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void MakeSureDataSourceExistsShouldNotAddIfExist()
		{
			var dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
			var dataSourceName = RandomName.Make();
			var appNhibConf = new Dictionary<string, string>();
			var statisticConnString = RandomName.Make();
			var dataSource = new FakeDataSource { DataSourceName = dataSourceName };
			dataSourcesFactory.Expect(x => x.Create(appNhibConf, statisticConnString)).Return(dataSource);
			var target = new ApplicationData(new Dictionary<string, string>(), new[]{dataSource}, null, null, dataSourcesFactory);
			target.Tenant(dataSourceName).Should().Be.SameInstanceAs(dataSource);
			target.MakeSureDataSourceExists(dataSourceName, RandomName.Make(), statisticConnString, appNhibConf);

			target.Tenant(dataSourceName).Should().Be.SameInstanceAs(dataSource);
			dataSourcesFactory.AssertWasNotCalled(x=> x.Create(appNhibConf, statisticConnString));
		}

		[Test]
        public void VerifyApplicationDataCanBeSet()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            ILoadPasswordPolicyService passwordPolicy = mocks.StrictMock<ILoadPasswordPolicyService>();

            IApplicationData target = new ApplicationData(_receivedSettings, dataSources, messBroker, passwordPolicy, null);
            Assert.AreEqual(_receivedSettings["HelpUrl"], target.AppSettings["HelpUrl"]);
            Assert.AreSame(_receivedSettings, target.AppSettings);
            Assert.AreSame(messBroker, target.Messaging);
            Assert.AreSame(passwordPolicy,target.LoadPasswordPolicyService);
        }

        /// <summary>
        /// Duplicate data sources are not allowed.
        /// </summary>
        [Test]
        [ExpectedException(typeof (DataSourceException))]
        public void DuplicateDataSourcesAreNotAllowed()
        {
            IDataSource dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("no dupl"), null,
                                                    null);
            IList<IDataSource> dataSourceList = new List<IDataSource>();
            dataSourceList.Add(dataSource);
            dataSourceList.Add(dataSource);
						using (new ApplicationData(_receivedSettings, new ReadOnlyCollection<IDataSource>(dataSourceList), null, null, null)) { }
        }


		[Test]
		public void CanFindDataSourceByTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new ApplicationData(_receivedSettings, new[] { ds }, null, null, null);
			target.Tenant(dsName).Should().Be.SameInstanceAs(ds);
		}

		[Test]
		public void MissingDataSourceTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new ApplicationData(_receivedSettings, new[] { ds }, null, null, null);
			target.Tenant("something else").Should().Be.Null();
		}

		[Test]
		public void NoDataSourceTenant()
		{
			var target = new ApplicationData(_receivedSettings, Enumerable.Empty<IDataSource>(), null, null, null);
			target.Tenant("something").Should().Be.Null();
		}

        [Test]
        public void VerifyDispose()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            IList<IDataSource> dsList = new List<IDataSource>();
            IUnitOfWorkFactory ds1App = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWorkFactory ds2App = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWorkFactory ds1Stat = mocks.StrictMock<IUnitOfWorkFactory>();
						dsList.Add(new DataSource(ds1App, ds1Stat, null));
						dsList.Add(new DataSource(ds2App, null, null));


            using (mocks.Record())
            {
                //just dummy names
                Expect.Call(ds1App.Name).Return("1").Repeat.Any();
                Expect.Call(ds2App.Name).Return("2").Repeat.Any();

                //dispose uowFactories
                ds1App.Dispose();
                ds2App.Dispose();
                ds1Stat.Dispose();
                //dispose mess broker
                messBroker.Dispose();
            }
            using (mocks.Playback())
            {
                ApplicationData target = new ApplicationData(_receivedSettings,
																														 new ReadOnlyCollection<IDataSource>(dsList), messBroker, null, null);
                target.Dispose();
            }
        }

		[Test]
		public void DoActionOnAllTenants()
		{
			var ds1 = MockRepository.GenerateMock<IDataSource>();
			var ds2 = MockRepository.GenerateMock<IDataSource>();
			var target = new ApplicationData(null, new[] { ds1, ds2 }, null, null, null);

			target.DoOnAllTenants_AvoidUsingThis(tenant => tenant.ResetStatistic());

			ds1.AssertWasCalled(x => x.ResetStatistic());
			ds2.AssertWasCalled(x => x.ResetStatistic());
		}
    }
}