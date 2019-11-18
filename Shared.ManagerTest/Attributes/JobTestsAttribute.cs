using System.Configuration;
using Autofac;
using log4net;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Policies;
using Stardust.Manager.Timers;

namespace ManagerTest.Attributes
{
	public class JobTestsAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			ManagerConfiguration config = new ManagerConfiguration(
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, "Route", 60, 20, 10, 1, 24,1);

			builder.RegisterInstance(config).SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<HalfNodesAffinityPolicy>().SingleInstance();
			builder.RegisterType<JobManager>().SingleInstance();
			builder.RegisterType<TestHelper>().SingleInstance();

			builder.RegisterType<NodeManager>();

			builder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			builder.RegisterType<JobRepositoryCommandExecuter>().SingleInstance();
			builder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();

			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance();
			builder.RegisterType<JobPurgeTimerFake>().As<JobPurgeTimer>().SingleInstance();
			builder.RegisterType<NodePurgeTimerFake>().As<NodePurgeTimer>().SingleInstance();
			builder.Register(c => new FakeLogger()).As<FakeLogger>().As<ILog>().SingleInstance();
		}
	}
}