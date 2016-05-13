using System.Configuration;
using System.Timers;
using Autofac;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;

namespace ManagerTest.Attributes
{
	public class JobTestsAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			ManagerConfiguration config = new ManagerConfiguration(
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, "Route", 60, 20, 10, 1, 24);

			builder.RegisterInstance(config).SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<CreateSqlCommandHelper>().SingleInstance();
			builder.RegisterType<JobManager>().SingleInstance();

			builder.RegisterType<NodeManager>();

			builder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			builder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();

			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance();
			builder.RegisterType<PurgeTimerFake>().As<Timer>().SingleInstance();
		}
	}
}