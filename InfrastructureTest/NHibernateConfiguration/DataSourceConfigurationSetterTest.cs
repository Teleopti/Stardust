using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.SqlAzure;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class DataSourceConfigurationSetterTest
	{
		[Test]
		public void ShouldHaveStaticDefaultValuesSet()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(true, string.Empty, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.Dialect).Should().Be.EqualTo(typeof(MsSql2008Dialect).AssemblyQualifiedName);
			cfg.GetProperty(Environment.DefaultSchema).Should().Be.EqualTo("dbo");
			cfg.GetProperty(Environment.SessionFactoryName).Should().Be.EqualTo("[not set]");
			cfg.GetProperty(Environment.SqlExceptionConverter).Should().Be.EqualTo(typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
			cfg.GetProperty(Environment.ConnectionDriver).Should().Be.EqualTo(typeof(SqlAzureClientDriverWithLogRetries).AssemblyQualifiedName);
			cfg.GetProperty(Environment.TransactionStrategy)
				.Should()
				.Be.EqualTo(typeof (ReliableAdoNetTransactionFactory).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldSetCacheConfig()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(true, null, null, new ConfigReader());
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
			var target = new dataSourceConfigurationSetterForTest(true, null, "application name", new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain(@"Application Name=""application name""");
		}

		[Test]
		public void ShouldNotOverrideApplicationNameOnConnectionString()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionString,
								 "Data Source=teleopti730;Initial Catalog=PBI17774_Demoreg_TeleoptiCCC7;user id=sa;password=cadadi;Application Name=Teleopti.CCC.Client");
			var target = new dataSourceConfigurationSetterForTest(true, null, "application name", new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain("Application Name=Teleopti.CCC.Client");
		}

		[Test]
		public void ShouldSetNoCacheConfig()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("false");
		}

		[Test]
		public void ShouldUseLatency()
		{
			var cfg = new Configuration();
			var configReader = new FakeConfigReader();
			configReader.FakeSetting("latency", "3");

			var target = new dataSourceConfigurationSetterForTest(false, null, null, configReader);

			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionDriver).Should().Be.EqualTo(typeof(TeleoptiLatencySqlDriver).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldNotUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Not.Contain("Distributed");
		}

		[Test]
		public void ShouldSetSessionContext()
		{
			const string sessionContext = "roger";
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, sessionContext, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.CurrentSessionContextClass).Should().Be.EqualTo(sessionContext);
		}

		[Test]
		public void ShouldNotSetNullSessionContext()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.Properties.ContainsKey(Environment.CurrentSessionContextClass).Should().Be.False();
		}

		[Test]
		public void ShouldNotSetEmptySessionContext()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, string.Empty, null, new ConfigReader());
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
			cfg.SetProperty(Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			var target = new dataSourceConfigurationSetterForTest(false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			foreach (var key in keys)
			{
				cfg.Properties[key].Should().Be.EqualTo(cfgValue);
			}
		}

		[Test]
		public void ShouldNotChangeDefinedDialect()
		{
			var dialect = typeof(MsSql2008Dialect).AssemblyQualifiedName;
			var cfg = new Configuration();
			cfg.SetProperty(Environment.Dialect, dialect);
			var target = new dataSourceConfigurationSetterForTest(false, null, null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.Dialect).Should().Be.EqualTo(dialect);
		}

		[Test]
		public void VerifyEtlConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForEtl();
			target.UseSecondLevelCache.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Wfm.Etl");
		}

		[Test]
		public void VerifyApplicationConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForApplicationConfig();
			target.UseSecondLevelCache.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.ApplicationConfiguration");
		}

		[Test]
		public void VerifySdkConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForSdk();
			target.UseSecondLevelCache.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Wfm.Sdk.Host");
		}

		[Test]
		public void VerifyServiceBusConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForServiceBus();
			target.UseSecondLevelCache.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Wfm.ServiceBus.Host");
		}

		[Test]
		public void VerifyWebConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForWeb();
			target.UseSecondLevelCache.Should().Be.True();
			target.SessionContext.Should().Be.EqualTo(typeof(TeleoptiSessionContext).AssemblyQualifiedName);
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Wfm.Web");
		}

		[Test]
		public void VerifyDesktopConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForDesktop();
			target.UseSecondLevelCache.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Teleopti.Wfm.SmartClientPortal.Shell");
		}


		private class dataSourceConfigurationSetterForTest : DataSourceConfigurationSetter
		{
			public dataSourceConfigurationSetterForTest(bool useSecondLevelCache, string sessionContext, string applicationName, IConfigReader configReader)
				: base(useSecondLevelCache, sessionContext, applicationName, configReader)
			{
			}
		}
	}
}