using Autofac;
using log4net;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Timers;
using Stardust.Manager.Validations;

namespace ManagerTest.Attributes
{
	public class ManagerActionExecutorValidationTestAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			ManagerConfiguration managerConfiguration = new ManagerConfiguration("connectionstring", "route", 60, 20, 1, 1, 1,1);
			builder.RegisterInstance(managerConfiguration).As<ManagerConfiguration>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();
			builder.RegisterType<FakeJobRepository>().As<IJobRepository>().SingleInstance();
            builder.RegisterType<FakeWorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();
            builder.RegisterType<JobManager>().As<IJobManager>().SingleInstance();
			builder.RegisterType<NodeManager>().SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<JobPurgeTimerFake>().As<JobPurgeTimer>().SingleInstance();
			builder.RegisterType<NodePurgeTimerFake>().As<NodePurgeTimer>().SingleInstance();
			builder.RegisterType<FakeLogger>().As<ILog>().SingleInstance();
            builder.RegisterType<ManagerActionExecutor>();
		}
	}
} 