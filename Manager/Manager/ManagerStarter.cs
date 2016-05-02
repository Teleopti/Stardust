using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
		public void Start(ManagerConfiguration managerConfiguration, IComponentContext componentContext)
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(managerConfiguration).SingleInstance();

			builder.RegisterType<NodeManager>().SingleInstance();
			builder.RegisterType<JobManager>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<CreateSqlCommandHelper>().SingleInstance();
			builder.RegisterType<HttpSender>().As<IHttpSender>().SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			builder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();

			builder.RegisterApiControllers(typeof (ManagerController).Assembly);

			builder.Update(componentContext.ComponentRegistry);
		}
	}
}