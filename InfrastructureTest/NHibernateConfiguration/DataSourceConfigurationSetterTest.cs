using System.Collections.Specialized;
using NHibernate.Cfg;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class DataSourceConfigurationSetterTest
	{
		[Test]
		public void ShouldHaveStaticDefaultValuesSet()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(true, true, string.Empty, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.Dialect).Should().Be.EqualTo("NHibernate.Dialect.MsSql2005Dialect");
			cfg.GetProperty(Environment.ConnectionProvider).Should().Be.EqualTo(typeof(TeleoptiDriverConnectionProvider).AssemblyQualifiedName);
			cfg.GetProperty(Environment.DefaultSchema).Should().Be.EqualTo("dbo");
			cfg.GetProperty(Environment.SessionFactoryName).Should().Be.EqualTo("[not set]");
			cfg.GetProperty(Environment.SqlExceptionConverter).Should().Be.EqualTo(typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
			cfg.GetProperty(Environment.ConnectionDriver).Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldSetCacheConfig()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(true, false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.CacheProvider).Should().Contain("RtMemoryCacheProvider");
			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("true");
			cfg.GetProperty(Environment.UseQueryCache).Should().Be.EqualTo("true");
		}

		[Test]
		public void ShouldSetApplicationNameOnConnectionString()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionString,
			                "Data Source=teleopti730;Initial Catalog=PBI17774_Demoreg_TeleoptiCCC7;user id=sa;password=cadadi");
			var target = new dataSourceConfigurationSetterForTest(true, false, null, "application name", new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain(@"Application Name=""application name""");
		}

		[Test]
		public void ShouldNotOverrideApplicationNameOnConnectionString()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionString,
								 "Data Source=teleopti730;Initial Catalog=PBI17774_Demoreg_TeleoptiCCC7;user id=sa;password=cadadi;Application Name=Teleopti.CCC.Client");
			var target = new dataSourceConfigurationSetterForTest(true, false, null, "application name", new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain("Application Name=Teleopti.CCC.Client");
		}

		[Test]
		public void ShouldSetNoCacheConfig()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("false");
		}

		[Test]
		public void ShouldUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, true, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Be.EqualTo(typeof(TeleoptiDistributedTransactionFactory).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldUseLatency()
		{
			var cfg = new Configuration();
			var configReader = MockRepository.GenerateMock<IConfigReader>();
			configReader.Expect(cr => cr.AppSettings).Return(new NameValueCollection { { "latency", "3" } });

			var target = new dataSourceConfigurationSetterForTest(false, true, null, null, configReader);

			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionDriver).Should().Be.EqualTo(typeof(TeleoptiLatencySqlDriver).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldNotUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Be.EqualTo("NHibernate.Transaction.AdoNetTransactionFactory, NHibernate");
		}

		[Test]
		public void ShouldSetSessionContext()
		{
			const string sessionContext = "roger";
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, sessionContext, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.CurrentSessionContextClass).Should().Be.EqualTo(sessionContext);
		}

		[Test]
		public void ShouldNotSetNullSessionContext()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.Properties.ContainsKey(Environment.CurrentSessionContextClass).Should().Be.False();
		}

		[Test]
		public void ShouldNotSetEmptySessionContext()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, string.Empty, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.Properties.ContainsKey(Environment.CurrentSessionContextClass).Should().Be.False();
		}

		[Test]
		public void ShouldNotChangeDefinedDefaultConfig()
		{
			const string cfgValue = "user defined";
			var cfg = new Configuration();
			var keys = new[]
			           	{
			           		Environment.ConnectionProvider,
			           		Environment.DefaultSchema,
			           		Environment.ProxyFactoryFactoryClass,
			           		Environment.SqlExceptionConverter,
			           		Environment.CacheProvider,
			           		Environment.UseSecondLevelCache,
			           		Environment.UseQueryCache,
			           		Environment.TransactionStrategy,
			           		Environment.CurrentSessionContextClass,
			           		Environment.SessionFactoryName
			           	};
			foreach (var key in keys)
			{
				cfg.SetProperty(key, cfgValue);
			}
			cfg.SetProperty(Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			foreach (var key in keys)
			{
				cfg.Properties[key].Should().Be.EqualTo(cfgValue);
			}
		}

		[Test]
		public void ShouldNotChangeDefinedDialect()
		{
			const string dialect = "NHibernate.Dialect.MsSql2008Dialect";
			var cfg = new Configuration();
			cfg.SetProperty(Environment.Dialect, dialect);
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.Dialect).Should().Be.EqualTo(dialect);
		}

		[Test]
		public void VerifyEtlConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForEtl();
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Analytics.ETL.nhib");
		}

		[Test]
		public void VerifyApplicationConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForApplicationConfig();
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.ApplicationConfiguration");
		}

		[Test]
		public void VerifySdkConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForSdk();
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Ccc.Sdk.Host");
		}

		[Test]
		public void VerifyServiceBusConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForServiceBus();
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.True();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Ccc.ServiceBus.Host");
		}

		[Test]
		public void VerifyWebConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForWeb();
			target.UseSecondLevelCache.Should().Be.True();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Ccc.Web");
		}

		[Test]
		public void VerifyDesktopConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForDesktop();
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Ccc.SmartClientPortal.Shell");
		}


		private class dataSourceConfigurationSetterForTest : DataSourceConfigurationSetter
		{
			public dataSourceConfigurationSetterForTest(bool useSecondLevelCache, bool useDistributedTransactionFactory, string sessionContext, string applicationName, IConfigReader configReader)
				: base(useSecondLevelCache, useDistributedTransactionFactory, sessionContext, applicationName, configReader)
			{
			}
		}
	}
}