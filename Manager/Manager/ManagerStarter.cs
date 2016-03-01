using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Manager.Interfaces;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
		public void Start(IManagerConfiguration managerConfiguration, IComponentContext componentContext)
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(managerConfiguration).As<IManagerConfiguration>();

			builder.RegisterType<NodeManager>().As<INodeManager>();

			builder.RegisterType<JobManager>().SingleInstance();

			builder.RegisterType<HttpSender>().As<IHttpSender>();
			 
			builder.Register(c => new JobRepository(managerConfiguration.ConnectionString))
							.As<IJobRepository>();

			builder.Register(c => new WorkerNodeRepository(managerConfiguration.ConnectionString))
							.As<IWorkerNodeRepository>();

			builder.RegisterApiControllers(typeof (ManagerController).Assembly);		

			builder.Update(componentContext.ComponentRegistry);
		}
	}
}