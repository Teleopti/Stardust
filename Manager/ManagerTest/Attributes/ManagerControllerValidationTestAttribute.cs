using Autofac;
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
			ManagerConfiguration managerConfiguration = new ManagerConfiguration("connectionstring", "route", 60, 20, 10, 1, 24);
			builder.RegisterInstance(managerConfiguration).As<ManagerConfiguration>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();
			builder.Register(c => new FakeJobRepository()).As<IJobRepository>();
			builder.Register(c => new FakeWorkerNodeRepository()).As<IWorkerNodeRepository>();
			builder.RegisterType<ManagerController>();
			builder.RegisterType<JobManager>().SingleInstance();
			builder.RegisterType<NodeManager>().SingleInstance();
		}
	}
}