﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
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
		private string testFile;
		private IEnversConfiguration enversConfiguration;

		[SetUp]
		public void Setup()
		{
			enversConfiguration = MockRepository.GenerateMock<IEnversConfiguration>();
			target = new DataSourcesFactory(enversConfiguration, new List<IDenormalizer>(), DataSourceConfigurationSetter.ForTest);
			string currDirectory = Directory.GetCurrentDirectory();
			testFile = currDirectory + "test.hbm.xml";
		}

		[TearDown]
		public void Teardown()
		{
			if (File.Exists(testFile))
				File.Delete(testFile);
		}

		[Test]
		public void VerifyFileBased()
		{
			string correctMatrix = @"<matrix name=""matrixName""><connectionString>" + ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";

			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(xmlText("test", correctMatrix));
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
			wasSuccess(success);
			Assert.AreEqual("test", res.Application.Name);
			Assert.AreEqual("matrixName", res.Statistic.Name);

			Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
			Assert.IsInstanceOf<NHibernateUnitOfWorkMatrixFactory>(res.Statistic);
		}

		[Test]
		public void VerifyXmlBased()
		{
			string correctMatrix = @"<matrix name=""matrixName""><connectionString>" + ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";

			string xmlString = xmlText("test", correctMatrix);

			using (var xmlReader = new XmlTextReader(xmlString, XmlNodeType.Document, null))
			{
				XElement nhibernateXmlConfiguration = XElement.Load(xmlReader);
				IDataSource res = target.Create(nhibernateXmlConfiguration, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
				Assert.AreEqual("test", res.Application.Name);
				Assert.AreEqual("matrixName", res.Statistic.Name);
				Assert.IsNull(res.OriginalFileName);

				Assert.IsInstanceOf<NHibernateUnitOfWorkFactory>(res.Application);
				Assert.IsInstanceOf<NHibernateUnitOfWorkMatrixFactory>(res.Statistic);
			}
		}

		[Test]
		public void VerifyXmlBasedWithDistributedTransaction()
		{
			target = new DataSourcesFactory(enversConfiguration, new List<IDenormalizer>(), DataSourceConfigurationSetter.ForTest);
			string correctMatrix = @"<matrix name=""matrixName""><connectionString>" + ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";

			string xmlString = xmlText("test", correctMatrix);

			XmlTextReader xmlReader = new XmlTextReader(xmlString, XmlNodeType.Document, null);

			XElement nhibernateXmlConfiguration = XElement.Load(xmlReader);

			IDataSource res = target.Create(nhibernateXmlConfiguration, ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);

			Assert.IsNotNull(res.Application, "Application is null, shouldn't be!");
			var factoryField = typeof(NHibernateUnitOfWorkFactory).GetProperty("SessionFactory",
																								  BindingFlags.NonPublic |
																								  BindingFlags.Instance);
			Assert.IsNotNull(factoryField, "Factory field is null, shouldn't be!");
			var sessionFactory = (ISessionFactoryImplementor)factoryField.GetValue(res.Application, BindingFlags.NonPublic |
																								  BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);
			Assert.IsNotNull(sessionFactory, "Session factory is null, shouldn't be!");
			Assert.IsInstanceOf<AdoNetWithDistributedTransactionFactory>(sessionFactory.TransactionFactory);
		}

		[Test]
		public void VerifyNoNames()
		{
			string matrix = @"<matrix><connectionString>" + ConnectionStringHelper.ConnectionStringUsedInTestsMatrix + @"</connectionString></matrix>";

			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(xmlText(string.Empty, matrix));
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
			wasSuccess(success);
			Assert.AreEqual(DataSourcesFactory.NoDataSourceName, res.Application.Name);
			Assert.AreEqual(DataSourcesFactory.NoDataSourceName, res.Statistic.Name);
		}

		[Test]
		[ExpectedException(typeof(DataSourceException))]
		public void VerifyDataSourceElementExists()
		{
			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(@"<gurka></gurka>");
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
			wasSuccess(success);
		}


		[Test]
		[ExpectedException(typeof(DataSourceException))]
		public void VerifyConnectionStringExists()
		{
			const string missingConnectionString = @"<matrix name=""matrixName""></matrix>";
			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(xmlText("test", missingConnectionString));
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
			wasSuccess(success);
		}


		[Test]
		[ExpectedException(typeof(DataSourceException))]
		public void VerifyConnectionStringIsNotEmpty()
		{
			const string emptyConnectionString = @"<matrix name=""matrixName""><connectionString>  </connectionString></matrix>";
			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(xmlText("test", emptyConnectionString));
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
			wasSuccess(success);
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

			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(xmlTextWithAuthenticationSettings(authenticationSettings));
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
			wasSuccess(success);

			Assert.IsInstanceOf<AuthenticationSettings>(res.AuthenticationSettings);
			Assert.IsTrue(LogOnModeOption.Win == res.AuthenticationSettings.LogOnMode);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pekar"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hib"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Galet"), Test]
		public void VerifyNhibWithValidConnectionStringMenDenPekarGalet()
		{
			const string authenticationSettings = @"<authentication><logonMode>win</logonMode> <!-- win or mix --></authentication>";

			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(nonValidXmlTextWithAuthenticationSettings(authenticationSettings));
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
			wasSuccess(success);
		}




		[Test]
		public void VerifyAuthenticationSettingsFileBasedWithoutEntry()
		{
			string authenticationSettings = string.Empty;

			using (StreamWriter file1 = new StreamWriter(testFile))
			{
				file1.WriteLine(xmlTextWithAuthenticationSettings(authenticationSettings));
			}
			IDataSource res;
			bool success = target.TryCreate(testFile, out res);
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

		private static string xmlText(string applicationDataSourceName,
											 string matrixInfo)
		{
			return string.Concat(
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
		}

		private static string xmlTextWithAuthenticationSettings(string authenticationSettings)
		{
			return string.Concat(
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
		}

		private static string nonValidXmlTextWithAuthenticationSettings(string authenticationSettings)
		{
			return string.Concat(
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
		}

		private void wasSuccess(bool success)
		{
			enversConfiguration.AssertWasCalled(x => x.Configure(null), options => options.IgnoreArguments());
			success.Should().Be.EqualTo(success);
		}
	}
}
