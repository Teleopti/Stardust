using System.Collections.Generic;
using System.Xml.Linq;
using NHibernate.Engine;
using NHibernate.Transaction;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
			target = new DataSourcesFactory(enversConfiguration, new List<IMessageSender>(), DataSourceConfigurationSetter.ForTest());
		}

		[Test]
		public void VerifyFileBased()
		{
			string correctMatrix = @"<matrix name=""matrixName""><connectionString>" + ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";
			var xElement = xmlText("test", correctMatrix);
			IDataSource res;
			bool success = target.TryCreate(xElement, out res);
			wasSuccess(success);
			Assert.AreEqual("test", res.Application.Name);
			Assert.AreEqual("matrixName", res.Statistic.Name);

			Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
			Assert.IsInstanceOf<NHibernateUnitOfWorkMatrixFactory>(res.Statistic);
		}

		[Test]
		public void VerifyXmlBased()
		{
			string correctMatrix = @"<matrix name=""matrixName""><connectionString>" +
			                       ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";

			XElement nhibernateXmlConfiguration = xmlText("test", correctMatrix);
			IDataSource res;
			target.TryCreate(nhibernateXmlConfiguration, out res);
			Assert.AreEqual("test", res.Application.Name);
			Assert.AreEqual("matrixName", res.Statistic.Name);
			Assert.IsNull(res.OriginalFileName);

			Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
			Assert.IsInstanceOf<NHibernateUnitOfWorkMatrixFactory>(res.Statistic);
		}

		[Test]
		public void VerifyXmlBasedWithDistributedTransaction()
		{
			target = new DataSourcesFactory(enversConfiguration, new List<IMessageSender>(), DataSourceConfigurationSetter.ForTest());
			string correctMatrix = @"<matrix name=""matrixName""><connectionString>" + ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";

			var nhibernateXmlConfiguration = xmlText("test", correctMatrix);

			IDataSource res;
			target.TryCreate(nhibernateXmlConfiguration, out res);
			var sessionFactory = (ISessionFactoryImplementor)((NHibernateUnitOfWorkFactory)res.Application).SessionFactory;
			Assert.IsInstanceOf<AdoNetTransactionFactory>(sessionFactory.TransactionFactory);
		}

		[Test]
		public void VerifyNoNames()
		{
			string matrix = @"<matrix><connectionString>" + ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";

			IDataSource res;
			bool success = target.TryCreate(xmlText(string.Empty, matrix), out res);
			wasSuccess(success);
			Assert.AreEqual(DataSourcesFactory.NoDataSourceName, res.Application.Name);
			Assert.AreEqual(DataSourcesFactory.NoDataSourceName, res.Statistic.Name);
		}

		[Test]
		[ExpectedException(typeof(DataSourceException))]
		public void VerifyMissingSesssionFactoryElement()
		{
			IDataSource res;
			target.TryCreate(XElement.Parse("<gurka></gurka>"), out res);
		}

		[Test]
		public void TestCreateWithDictionaryNoMatrix()
		{
			IDataSource res = target.Create(nHibSettings(), string.Empty);
			Assert.AreEqual(DataSourcesFactory.NoDataSourceName, res.Application.Name);
			Assert.IsNull(res.Statistic);
			Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
		}

		[Test]
		public void TestCreateWithDictionaryWithMatrix()
		{
			IDataSource res = target.Create(nHibSettings(), ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			Assert.AreEqual(DataSourcesFactory.NoDataSourceName, res.Application.Name);
			Assert.AreEqual(DataSourcesFactory.NoDataSourceName, res.Statistic.Name);
			Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
			Assert.IsInstanceOf<NHibernateUnitOfWorkMatrixFactory>(res.Statistic);
		}

		[Test]
		public void ShouldAddApplicationNameToConnectionString()
		{
			var res = target.Create(nHibSettings(), ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			using (var appSession = ((NHibernateUnitOfWorkFactory)res.Application).SessionFactory.OpenSession())
			{
				appSession.Connection.ConnectionString.Should().Contain("unit tests");
			}
			using (var appSession = ((NHibernateUnitOfWorkFactory)res.Statistic).SessionFactory.OpenSession())
			{
				appSession.Connection.ConnectionString.Should().Contain("unit tests");
			}
		}

		[Test]
		public void VerifyCreateDropSchemaWorks()
		{
			//hard to test
			Assert.IsNotNull(target.Create(nHibSettings(), ConnectionStringHelper.ConnectionStringUsedInTestsMatrix));
			//target.CreateSchema(); --nope - will fail if david's script is used
		}

		[Test]
		public void VerifyCreateDropSchemaIfNoMatrix()
		{
			//hard to test
			Assert.IsNotNull(target.Create(nHibSettings(), null));
			//target.CreateSchema(); --nope - will fail if david's script is used
		}

		[Test]
		public void VerifyAuthenticationSettingsFileBasedWithEntry()
		{
			const string authenticationSettings = @"<authentication><logonMode>win</logonMode> <!-- win or mix --></authentication>";
			var xmlElement = xmlTextWithAuthenticationSettings(authenticationSettings);
			IDataSource res;
			bool success = target.TryCreate(xmlElement, out res);
			wasSuccess(success);

			Assert.IsInstanceOf<AuthenticationSettings>(res.AuthenticationSettings);
			Assert.IsTrue(LogOnModeOption.Win == res.AuthenticationSettings.LogOnMode);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pekar"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hib"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Galet"), Test]
		public void VerifyNhibWithValidConnectionStringMenDenPekarGalet()
		{
			const string authenticationSettings = @"<authentication><logonMode>win</logonMode> <!-- win or mix --></authentication>";

			IDataSource res;
			target.TryCreate(nonValidXmlTextWithAuthenticationSettings(authenticationSettings), out res)
				.Should().Be.False();
		}

		[Test]
		public void VerifyAuthenticationSettingsFileBasedWithoutEntry()
		{
			string authenticationSettings = string.Empty;

			IDataSource res;
			bool success = target.TryCreate(xmlTextWithAuthenticationSettings(authenticationSettings), out res);
			wasSuccess(success);

			Assert.IsInstanceOf<AuthenticationSettings>(res.AuthenticationSettings);
			Assert.IsTrue(LogOnModeOption.Mix == res.AuthenticationSettings.LogOnMode);
		}

		[Test]
		public void VerifyAuthenticationSettingsWithoutFile()
		{
			IDataSource res = target.Create(nHibSettings(), ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);

			Assert.IsInstanceOf<AuthenticationSettings>(res.AuthenticationSettings);
			Assert.IsTrue(LogOnModeOption.Mix == res.AuthenticationSettings.LogOnMode);

		}

		private static IDictionary<string, string> nHibSettings()
		{
			IDictionary<string, string> ret = new Dictionary<string, string>();
			ret.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
			ret.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
			ret.Add("connection.connection_string", ConnectionStringHelper.ConnectionStringUsedInTests);
			ret.Add("show_sql", "false");
			ret.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");

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
			ConnectionStringHelper.ConnectionStringUsedInTests,
				 @"</property>
								<property name=""show_sql"">false</property> 
								<property name=""dialect"">NHibernate.Dialect.MsSql2005Dialect</property>
								<property name=""default_schema"">nhtest2.dbo</property>
							 </session-factory >
						  </hibernate-configuration>
						  ", matrixInfo, @"
						</datasource>
					 ");
			return XElement.Parse(str);
		}

		private static XElement xmlTextWithAuthenticationSettings(string authenticationSettings)
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
			ConnectionStringHelper.ConnectionStringUsedInTests,
				 @"</property>
								<property name=""show_sql"">false</property> 
								<property name=""dialect"">NHibernate.Dialect.MsSql2005Dialect</property>
								<property name=""default_schema"">nhtest2.dbo</property>
							 </session-factory >
						  </hibernate-configuration>
						  <matrix name=""matrixName""><connectionString>",
			ConnectionStringHelper.ConnectionStringUsedInTestsMatrix,
					  @"</connectionString></matrix>
						  ", authenticationSettings, @"
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
			ConnectionStringHelper.NonValidConnectionStringUsedInTests,
				 @"</property>
								<property name=""show_sql"">false</property> 
								<property name=""dialect"">NHibernate.Dialect.MsSql2005Dialect</property>
								<property name=""default_schema"">nhtest2.dbo</property>
							 </session-factory >
						  </hibernate-configuration>
						  <matrix name=""matrixName""><connectionString>",
			ConnectionStringHelper.ConnectionStringUsedInTestsMatrix,
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
