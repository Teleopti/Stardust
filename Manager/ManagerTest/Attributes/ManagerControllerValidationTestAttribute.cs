using System.Timers;
using Autofac;
using Autofac.Integration.WebApi;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

namespace ManagerTest.Attributes
{
	public class ManagerControllerValidationTestAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			ManagerConfiguration managerConfiguration = new ManagerConfiguration("connectionstring", "route", 60, 20, 1, 1, 1);
			builder.RegisterInstance(managerConfiguration).As<ManagerConfiguration>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();
			builder.Register(c => new FakeJobRepository()).As<IJobRepository>();
			builder.Register(c => new FakeWorkerNodeRepository()).As<IWorkerNodeRepository>();
			builder.RegisterApiControllers(typeof(ManagerController).Assembly);
			builder.RegisterType<JobManager>().SingleInstance();
			builder.RegisterType<NodeManager>().SingleInstance();
			builder.RegisterType<PurgeTimerFake>().As<Timer>().SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
		}
	}
}