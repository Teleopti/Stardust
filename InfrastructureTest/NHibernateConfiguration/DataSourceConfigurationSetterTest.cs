using NHibernate.Cfg;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class DataSourceConfigurationSetterTest
	{
		[Test]
		public void ShouldHaveStaticDefaultValuesSet()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(true, true, string.Empty, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.Dialect).Should().Be.EqualTo("NHibernate.Dialect.MsSql2005Dialect");
			cfg.GetProperty(Environment.ConnectionProvider).Should().Be.EqualTo(typeof(TeleoptiDriverConnectionProvider).AssemblyQualifiedName);
			cfg.GetProperty(Environment.DefaultSchema).Should().Be.EqualTo("dbo");
			cfg.GetProperty(Environment.SessionFactoryName).Should().Be.EqualTo("[not set]");
			cfg.GetProperty(Environment.SqlExceptionConverter).Should().Be.EqualTo(typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldSetCacheConfig()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(true, false, null, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.CacheProvider).Should().Be.EqualTo("NHibernate.Caches.SysCache.SysCacheProvider, NHibernate.Caches.SysCache");
			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("true");
			cfg.GetProperty(Environment.UseQueryCache).Should().Be.EqualTo("true");
		}

		[Test]
		public void ShouldSetApplicationNameOnConnectionString()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionString,
			                "Data Source=teleopti730;Initial Catalog=PBI17774_Demoreg_TeleoptiCCC7;user id=sa;password=cadadi");
			var target = new dataSourceConfigurationSetterForTest(true, false, null, "application name");
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain(@"Application Name=""application name""");
		}

		[Test]
		public void ShouldNotOverrideApplicationNameOnConnectionString()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionString,
								 "Data Source=teleopti730;Initial Catalog=PBI17774_Demoreg_TeleoptiCCC7;user id=sa;password=cadadi;Application Name=Teleopti.CCC.Client");
			var target = new dataSourceConfigurationSetterForTest(true, false, null, "application name");
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain("Application Name=Teleopti.CCC.Client");
		}

		[Test]
		public void ShouldSetNoCacheConfig()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("false");
		}

		[Test]
		public void ShouldUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, true, null, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Be.EqualTo(typeof(TeleoptiDistributedTransactionFactory).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldNotUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Be.EqualTo("NHibernate.Transaction.AdoNetTransactionFactory, NHibernate");
		}

		[Test]
		public void ShouldSetSessionContext()
		{
			const string sessionContext = "roger";
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, sessionContext, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.CurrentSessionContextClass).Should().Be.EqualTo(sessionContext);
		}

		[Test]
		public void ShouldNotSetNullSessionContext()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, null, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.Properties.ContainsKey(Environment.CurrentSessionContextClass).Should().Be.False();
		}

		[Test]
		public void ShouldNotSetEmptySessionContext()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(false, false, string.Empty, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.Properties.ContainsKey(Environment.CurrentSessionContextClass).Should().Be.False();
		}

		[Test]
		public void VerifyEtlConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForEtl;
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("ETL tool");
		}

		[Test]
		public void VerifyApplicationConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForApplicationConfig;
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Application Config");
		}

		[Test]
		public void VerifySdkConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForSdk;
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("SDK");
		}

		[Test]
		public void VerifyServiceBusConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForServiceBus;
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.True();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Service bus");
		}

		[Test]
		public void VerifyWebConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForWeb;
			target.UseSecondLevelCache.Should().Be.True();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure");
			target.ApplicationName.Should().Be.EqualTo("Web mytime");
		}

		[Test]
		public void VerifyDesktopConfig()
		{
			var target = (DataSourceConfigurationSetter)DataSourceConfigurationSetter.ForDesktop;
			target.UseSecondLevelCache.Should().Be.False();
			target.UseDistributedTransactionFactory.Should().Be.False();
			target.SessionContext.Should().Be.EqualTo("thread_static");
			target.ApplicationName.Should().Be.EqualTo("Desktop");
		}


		private class dataSourceConfigurationSetterForTest : DataSourceConfigurationSetter
		{
			public dataSourceConfigurationSetterForTest(bool useSecondLevelCache, bool useDistributedTransactionFactory, string sessionContext, string applicationName)
				: base(useSecondLevelCache, useDistributedTransactionFactory, sessionContext, applicationName)
			{
			}
		}
	}
}