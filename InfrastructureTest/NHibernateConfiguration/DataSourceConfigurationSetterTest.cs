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
			var target = new DataSourceConfigurationSetter(true, true, string.Empty);
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
			var target = new DataSourceConfigurationSetter(true, false, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.CacheProvider).Should().Be.EqualTo("NHibernate.Caches.SysCache.SysCacheProvider, NHibernate.Caches.SysCache");
			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("true");
			cfg.GetProperty(Environment.UseQueryCache).Should().Be.EqualTo("true");
		}

		[Test]
		public void ShouldSetNoCacheConfig()
		{
			var cfg = new Configuration();
			var target = new DataSourceConfigurationSetter(false, false, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("false");
		}

		[Test]
		public void ShouldUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new DataSourceConfigurationSetter(false, true, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Be.EqualTo(typeof(TeleoptiDistributedTransactionFactory).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldNotUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new DataSourceConfigurationSetter(false, false, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Be.EqualTo("NHibernate.Transaction.AdoNetTransactionFactory, NHibernate");
		}

		[Test]
		public void ShouldSetSessionContext()
		{
			const string sessionContext = "roger";
			var cfg = new Configuration();
			var target = new DataSourceConfigurationSetter(false, false, sessionContext);
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.CurrentSessionContextClass).Should().Be.EqualTo(sessionContext);
		}

		[Test]
		public void ShouldNotSetNullSessionContext()
		{
			var cfg = new Configuration();
			var target = new DataSourceConfigurationSetter(false, false, null);
			target.AddDefaultSettingsTo(cfg);

			cfg.Properties.ContainsKey(Environment.CurrentSessionContextClass).Should().Be.False();
		}

		[Test]
		public void ShouldNotSetEmptySessionContext()
		{
			var cfg = new Configuration();
			var target = new DataSourceConfigurationSetter(false, false, string.Empty);
			target.AddDefaultSettingsTo(cfg);

			cfg.Properties.ContainsKey(Environment.CurrentSessionContextClass).Should().Be.False();
		}
	}
}