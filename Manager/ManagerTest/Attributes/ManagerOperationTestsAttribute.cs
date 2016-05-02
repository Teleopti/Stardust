using System.Configuration;
using Autofac;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

namespace ManagerTest.Attributes
{
	public class ManagerOperationTestsAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();

			ManagerConfiguration config = new ManagerConfiguration()
			{
				ConnectionString = ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString
			};

			builder.RegisterInstance(config).SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<CreateSqlCommandHelper>().SingleInstance();
			builder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			builder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();

			builder.RegisterType<ManagerController>();

			builder.RegisterType<JobManager>().SingleInstance();
			builder.RegisterType<NodeManager>().SingleInstance();
		}
	}
}