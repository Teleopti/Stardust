using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[Category("BucketB")]
	public class DataSourcesFactoryTest
	{
		private IDataSourcesFactory target;

		[SetUp]
		public void Setup()
		{
			target = DataSourceFactoryFactory.MakeLegacyWay().Make();
		}

		[Test]
		public void TestCreateWithDictionaryNoMatrix()
		{
			IDataSource res = target.Create(nHibSettings(), string.Empty);
			Assert.AreEqual(DataSourceConfigurationSetter.NoDataSourceName, res.Application.Name);
			Assert.IsNull(res.Analytics);
			Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
		}
		
		[Test]
		public void ShouldAddApplicationNameToConnectionString()
		{
			var res = target.Create(nHibSettings(), InfraTestConfigReader.AnalyticsConnectionString());
			using (var appSession = ((NHibernateUnitOfWorkFactory)res.Application).SessionFactory.OpenSession())
			{
				appSession.Connection.ConnectionString.Should().Contain("unit tests");
			}
			using (var uow = res.Analytics.CreateAndOpenUnitOfWork())
			{
				uow.FetchSession().Connection.ConnectionString.Should().Contain("unit tests");
			}
		}

		private static IDictionary<string, string> nHibSettings()
		{
			IDictionary<string, string> ret = new Dictionary<string, string>();
			ret.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
			ret.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
			ret.Add("connection.connection_string", InfraTestConfigReader.ApplicationConnectionString());
			ret.Add("show_sql", "false");
			ret.Add("dialect", "NHibernate.Dialect.MsSql2008Dialect");

			return ret;
		}
	}
}
