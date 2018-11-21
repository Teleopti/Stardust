using System.Data.SqlClient;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Develop.Batflow
{
	public class HangfireRecurringJobTriggerCommand
	{
		public string Server;
		public string ApplicationDatabase;
		public string AnalyticsDatabase;
	}

	public class HangfireRecurringJobTrigger
	{
		public void Trigger(HangfireRecurringJobTriggerCommand command)
		{
			var builder = new ContainerBuilder();
			var config = new FakeConfigReader();
			config.FakeConnectionString("Hangfire", new SqlConnectionStringBuilder
			{
				DataSource = command.Server,
				IntegratedSecurity = true,
				InitialCatalog = command.AnalyticsDatabase
			}.ConnectionString);
			config.FakeConnectionString("Tenancy", new SqlConnectionStringBuilder
			{
				DataSource = command.Server,
				IntegratedSecurity = true,
				InitialCatalog = command.ApplicationDatabase
			}.ConnectionString);
			var args = new IocArgs(config) {DataSourceApplicationName = DataSourceApplicationName.ForTest()};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new CommonModule(configuration));
			using (var container = builder.Build())
			{
				container.Resolve<IHangfireClientStarter>().Start();
				container.Resolve<RecurringEventPublishings>().UpdatePublishings();
				container.Resolve<HangfireUtilities>().TriggerRecurringJobs();
			}
		}
	}
}