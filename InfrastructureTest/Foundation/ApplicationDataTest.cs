#region Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.FSharp.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

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
        public void VerifyApplicationDataCanBeSet()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            ILoadPasswordPolicyService passwordPolicy = mocks.StrictMock<ILoadPasswordPolicyService>();

            IApplicationData target = new ApplicationData(_receivedSettings, dataSources, messBroker, passwordPolicy, null);
            Assert.AreEqual(_receivedSettings["HelpUrl"], target.AppSettings["HelpUrl"]);
            Assert.AreSame(_receivedSettings, target.AppSettings);
				target.RegisteredDataSourceCollection.Should().Have.SameValuesAs(dataSources);
            Assert.AreSame(messBroker, target.Messaging);
            Assert.AreSame(passwordPolicy,target.LoadPasswordPolicyService);
        }

        [Test]
        public void VerifySingleDataSourceConstructor()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            IDataSource dataSource = mocks.StrictMock<IDataSource>();
            IApplicationData target = new ApplicationData(_receivedSettings, new[]{dataSource}, messBroker, null, null);
            Assert.AreSame(_receivedSettings, target.AppSettings);
            Assert.AreSame(dataSource, target.RegisteredDataSourceCollection.First());
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

        /// <summary>
        /// Verifies that multiple data sources can exist.
        /// </summary>
        [Test]
        public void VerifyMultipleDataSourcesCanExist()
        {
            IDataSource dataSource1 = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("one"), null, null);
            IDataSource dataSource2 = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("two"), null, null);
            IList<IDataSource> dataSourceList = new List<IDataSource>();
            dataSourceList.Add(dataSource1);
            dataSourceList.Add(dataSource2);
            ApplicationData target =
								new ApplicationData(_receivedSettings, new ReadOnlyCollection<IDataSource>(dataSourceList), null, null, null);
            Assert.AreEqual(2, target.RegisteredDataSourceCollection.Count());
            Assert.IsTrue(target.RegisteredDataSourceCollection.Contains(dataSource1));
            Assert.IsTrue(target.RegisteredDataSourceCollection.Contains(dataSource2));
        }


		[Test]
		public void CanFindDataSourceByTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new ApplicationData(_receivedSettings, new[] { ds }, null, null, null);
			target.DataSource(dsName).Should().Be.SameInstanceAs(ds);
		}

		[Test, Ignore("Enable this test when we're getting further with tenancy stuff")]
		public void MissingDataSourceTenant()
		{
			var dsName = Guid.NewGuid().ToString();
			var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory(dsName), null, null);
			var target = new ApplicationData(_receivedSettings, new[] { ds }, null, null, null);
			target.DataSource("something else").Should().Be.Null();
		}

		[Test]
		public void NoDataSourceTenant()
		{
			var target = new ApplicationData(_receivedSettings, null, null, null, null);
			target.DataSource("something").Should().Be.Null();
		}

        [Test]
        public void VerifyDispose()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            IList<IDataSource> dsList = new List<IDataSource>();
            IDataSource ds1 = mocks.StrictMock<IDataSource>();
            IDataSource ds2 = mocks.StrictMock<IDataSource>();
            dsList.Add(ds1);
            dsList.Add(ds2);
            IUnitOfWorkFactory ds1App = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWorkFactory ds2App = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWorkFactory ds1Stat = mocks.StrictMock<IUnitOfWorkFactory>();


            using (mocks.Record())
            {
                //just dummy names
                Expect.Call(ds1App.Name).Return("1").Repeat.Any();
                Expect.Call(ds2App.Name).Return("2").Repeat.Any();

                //dispose uowFactories
                Expect.Call(ds1.Statistic)
                    .Return(ds1Stat).Repeat.Any();
                Expect.Call(ds2.Statistic)
                    .Return(null).Repeat.Any();
                Expect.Call(ds1.Application)
                    .Return(ds1App).Repeat.Any();
                Expect.Call(ds2.Application)
                    .Return(ds2App).Repeat.Any();
                ds1.Dispose();
                ds2.Dispose();
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
    }
}