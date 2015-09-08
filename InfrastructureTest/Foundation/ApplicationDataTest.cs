using System;
using System.Collections.Generic;
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


namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
    [Category("LongRunning")]
    public class ApplicationDataTest
    {
        private MockRepository mocks;
        private IDictionary<string, string> _receivedSettings;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();

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
			var target = new ApplicationData(new Dictionary<string, string>(),  null, null, dataSourcesFactory);
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
			var target = new ApplicationData(new Dictionary<string, string>(), null, null, dataSourcesFactory);
			target.MakeSureDataSourceExists_UseOnlyFromTests(dataSource);

			target.Tenant(dataSourceName).Should().Be.SameInstanceAs(dataSource);
			dataSourcesFactory.AssertWasNotCalled(x=> x.Create(appNhibConf, statisticConnString));
		}

		[Test]
        public void VerifyApplicationDataCanBeSet()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            ILoadPasswordPolicyService passwordPolicy = mocks.StrictMock<ILoadPasswordPolicyService>();

            var target = new ApplicationData(_receivedSettings, messBroker, passwordPolicy, null);
			target.MakeSureDataSourceExists_UseOnlyFromTests(new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("test"), null, null));
            Assert.AreEqual(_receivedSettings["HelpUrl"], target.AppSettings["HelpUrl"]);
            Assert.AreSame(_receivedSettings, target.AppSettings);
            Assert.AreSame(messBroker, target.Messaging);
            Assert.AreSame(passwordPolicy,target.LoadPasswordPolicyService);
        }

		[Test]
		public void CanFindDataSourceByTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new ApplicationData(_receivedSettings, null, null, null);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds);
			target.Tenant(dsName).Should().Be.SameInstanceAs(ds);
		}

		[Test]
		public void MissingDataSourceTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new ApplicationData(_receivedSettings, null, null, null);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds);
			target.Tenant("something else").Should().Be.Null();
		}

		[Test]
		public void NoDataSourceTenant()
		{
			var target = new ApplicationData(_receivedSettings, null, null, null);
			target.Tenant("something").Should().Be.Null();
		}

        [Test]
        public void VerifyDispose()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            IUnitOfWorkFactory ds1App = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWorkFactory ds2App = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWorkFactory ds1Stat = mocks.StrictMock<IUnitOfWorkFactory>();
						var ds1 = new DataSource(ds1App, ds1Stat, null);
						var ds2 = new DataSource(ds2App, null, null);


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
                ApplicationData target = new ApplicationData(_receivedSettings, messBroker, null, null);
				target.MakeSureDataSourceExists_UseOnlyFromTests(ds1);
				target.MakeSureDataSourceExists_UseOnlyFromTests(ds2);
                target.Dispose();
            }
        }

		[Test]
		public void DoActionOnAllTenants()
		{
			var ds1 = MockRepository.GenerateMock<IDataSource>();
			var ds2 = MockRepository.GenerateMock<IDataSource>();
			var target = new ApplicationData(null, null, null, null);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds1);
			target.MakeSureDataSourceExists_UseOnlyFromTests(ds2);

			target.DoOnAllTenants_AvoidUsingThis(tenant => tenant.ResetStatistic());

			ds1.AssertWasCalled(x => x.ResetStatistic());
			ds2.AssertWasCalled(x => x.ResetStatistic());
		}
    }
}