using System.Collections.Generic;
using System.Xml.Linq;
using NHibernate.Engine;
using NHibernate.Transaction;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[Category("LongRunning")]
	public class DataSourcesFactoryTest
	{
		private IDataSourcesFactory target;
		private IEnversConfiguration enversConfiguration;

		[SetUp]
		public void Setup()
		{
			enversConfiguration = MockRepository.GenerateMock<IEnversConfiguration>();
			target = new DataSourcesFactory(enversConfiguration, new NoPersistCallbacks(), DataSourceConfigurationSetter.ForTest(), new CurrentHttpContext(), null);
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
		public void TestCreateWithDictionaryWithMatrix()
		{
			IDataSource res = target.Create(nHibSettings(), InfraTestConfigReader.AnalyticsConnectionString);
			Assert.AreEqual(DataSourceConfigurationSetter.NoDataSourceName, res.Application.Name);
			Assert.AreEqual(DataSourcesFactory.AnalyticsDataSourceName, res.Analytics.Name);
			Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
			Assert.IsInstanceOf<NHibernateUnitOfWorkMatrixFactory>(res.Analytics);
		}

		[Test]
		public void ShouldAddApplicationNameToConnectionString()
		{
			var res = target.Create(nHibSettings(), InfraTestConfigReader.AnalyticsConnectionString);
			using (var appSession = ((NHibernateUnitOfWorkFactory)res.Application).SessionFactory.OpenSession())
			{
				appSession.Connection.ConnectionString.Should().Contain("unit tests");
			}
			using (var appSession = ((NHibernateUnitOfWorkFactory)res.Analytics).SessionFactory.OpenSession())
			{
				appSession.Connection.ConnectionString.Should().Contain("unit tests");
			}
		}

		private static IDictionary<string, string> nHibSettings()
		{
			IDictionary<string, string> ret = new Dictionary<string, string>();
			ret.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
			ret.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
			ret.Add("connection.connection_string", InfraTestConfigReader.ConnectionString);
			ret.Add("show_sql", "false");
			ret.Add("dialect", "NHibernate.Dialect.MsSql2008Dialect");

			return ret;
		}

		private static XElement xmlText(string applicationDataSourceName,
											 string matrixInfo)
		{
			var str =  string.Concat(
				 @"<?xml version=""1.0"" encoding=""utf-8"" ?>
						 <datasource>
						  <hibernate-configuration  xmlns=""urn:nhibernate-configuration-2.2"" >
							 <session-factory name=""", applicationDataSourceName, @""">
								<!-- properties -->
								<property name=""connection.provider"">NHibernate.Connection.DriverConnectionProvider</property>
								<property name=""connection.driver_class"">NHibernate.Driver.SqlClientDriver</property>
								<property name=""connection.connection_string"">",
			InfraTestConfigReader.ConnectionString,
				 @"</property>
								<property name=""show_sql"">false</property> 
								<property name=""dialect"">NHibernate.Dialect.MsSql2008Dialect</property>
								<property name=""default_schema"">nhtest2.dbo</property>
							 </session-factory >
						  </hibernate-configuration>
						  ", matrixInfo, @"
						</datasource>
					 ");
			return XElement.Parse(str);
		}

		private static XElement nonValidXmlTextWithAuthenticationSettings(string authenticationSettings)
		{
			var str = string.Concat(
				 @"<?xml version=""1.0"" encoding=""utf-8"" ?>
						 <datasource>
						  <hibernate-configuration  xmlns=""urn:nhibernate-configuration-2.2"" >
							 <session-factory name=""test"">
								<!-- properties -->
								<property name=""connection.provider"">NHibernate.Connection.DriverConnectionProvider</property>
								<property name=""connection.driver_class"">NHibernate.Driver.SqlClientDriver</property>
								<property name=""connection.connection_string"">",
			InfraTestConfigReader.InvalidConnectionString,
				 @"</property>
								<property name=""show_sql"">false</property> 
								<property name=""dialect"">NHibernate.Dialect.MsSql2008Dialect</property>
								<property name=""default_schema"">nhtest2.dbo</property>
							 </session-factory >
						  </hibernate-configuration>
						  <matrix name=""matrixName""><connectionString>",
			InfraTestConfigReader.AnalyticsConnectionString,
					  @"</connectionString></matrix>
						  ", authenticationSettings, @"
						</datasource>
					 ");
			return XElement.Parse(str);
		}

		private void wasSuccess(bool result)
		{
			result.Should().Be.True();
			enversConfiguration.AssertWasCalled(x => x.Configure(null), options => options.IgnoreArguments());
		}
	}
}
