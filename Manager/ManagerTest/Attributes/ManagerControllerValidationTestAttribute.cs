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
			builder.RegisterType<ManagerConfiguration>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<NodeManager>().SingleInstance();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();
			builder.Register(c => new FakeJobRepository()).As<IJobRepository>();
			builder.Register(c => new FakeWorkerNodeRepository()).As<IWorkerNodeRepository>();
			builder.RegisterType<ManagerController>();
			builder.RegisterType<JobManager>().SingleInstance();
		}
	}
}