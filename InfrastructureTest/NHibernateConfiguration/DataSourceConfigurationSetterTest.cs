using NHibernate.Cfg;
using NHibernate.Dialect;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class DataSourceConfigurationSetterTest
	{
		[Test]
		public void ShouldHaveStaticDefaultValuesWithResilientDriverSet()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.Dialect).Should().Be.EqualTo(typeof(MsSql2008Dialect).AssemblyQualifiedName);
			cfg.GetProperty(Environment.DefaultSchema).Should().Be.EqualTo("dbo");
			cfg.GetProperty(Environment.SessionFactoryName).Should().Be.EqualTo("[not set]");
			cfg.GetProperty(Environment.SqlExceptionConverter).Should().Be.EqualTo(typeof(SqlServerExceptionConverter).AssemblyQualifiedName);
			cfg.GetProperty(Environment.ConnectionDriver).Should().Be.EqualTo(typeof(ResilientSql2008ClientDriver).AssemblyQualifiedName);
			cfg.GetProperty(Environment.TransactionStrategy)
				.Should()
				.Be.EqualTo(typeof(ResilientAdoNetTransactionFactory).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldSetApplicationNameOnConnectionString()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionString,
				"Data Source=teleopti730;Initial Catalog=PBI17774_Demoreg_TeleoptiCCC7;user id=sa;password=cadadi");
			var target = new dataSourceConfigurationSetterForTest("application name", new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain(@"Application Name=""application name""");
		}

		[Test]
		public void ShouldNotOverrideApplicationNameOnConnectionString()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionString,
				"Data Source=teleopti730;Initial Catalog=PBI17774_Demoreg_TeleoptiCCC7;user id=sa;password=cadadi;Application Name=Teleopti.CCC.Client");
			var target = new dataSourceConfigurationSetterForTest("application name", new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionString).Should().Contain("Application Name=Teleopti.CCC.Client");
		}

		[Test]
		public void ShouldSetNoCacheConfig()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.UseSecondLevelCache).Should().Be.EqualTo("false");
		}

		[Test]
		public void ShouldUseLatency()
		{
			var cfg = new Configuration();
			var configReader = new FakeConfigReader();
			configReader.FakeSetting("latency", "3");

			var target = new dataSourceConfigurationSetterForTest(null, configReader);

			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.ConnectionDriver).Should().Be.EqualTo(typeof(TeleoptiLatencySqlDriver).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldNotUseDistributedTransactionFactory()
		{
			var cfg = new Configuration();
			var target = new dataSourceConfigurationSetterForTest(null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);

			cfg.GetProperty(Environment.TransactionStrategy).Should().Not.Contain("Distributed");
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
				Environment.SessionFactoryName
			};
			foreach (var key in keys)
			{
				cfg.SetProperty(key, cfgValue);
			}

			cfg.SetProperty(Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			var target = new dataSourceConfigurationSetterForTest(null, new ConfigReader());
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
			var target = new dataSourceConfigurationSetterForTest(null, new ConfigReader());
			target.AddDefaultSettingsTo(cfg);
			cfg.GetProperty(Environment.Dialect).Should().Be.EqualTo(dialect);
		}

		private class dataSourceConfigurationSetterForTest : DataSourceConfigurationSetter
		{
			public dataSourceConfigurationSetterForTest(string applicationName, IConfigReader configReader)
				: base(new DataSourceApplicationName{Name = applicationName}, configReader)
			{
			}
		}
	}
}