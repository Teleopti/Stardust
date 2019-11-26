using System.Configuration;
using Autofac;
using log4net;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Policies;
using Stardust.Manager.Timers;
using Stardust.Manager.Validations;

namespace ManagerTest.Attributes
{
	public class ManagerOperationTestsAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();

			ManagerConfiguration config = new ManagerConfiguration(
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, "Route", 60, 20, 10, 1, 24, 1);

			builder.RegisterInstance(config).SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<HalfNodesAffinityPolicy>().SingleInstance();
			builder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			builder.RegisterType<JobRepositoryCommandExecuter>().SingleInstance();
			builder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();

			builder.RegisterType<ManagerController>();
			builder.RegisterType<FakeLogger>().As<ILog>().SingleInstance();
			builder.RegisterType<JobManager>().As<IJobManager>().SingleInstance();
			builder.RegisterType<NodeManager>().SingleInstance();
			builder.RegisterType<JobPurgeTimerFake>().As<JobPurgeTimer>().SingleInstance();
			builder.RegisterType<NodePurgeTimerFake>().As<NodePurgeTimer>().SingleInstance();
			builder.RegisterType<TestHelper>().SingleInstance();
		}
	}
}