using System.Threading;
using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
		private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

		public void Stop()
		{
			QuitEvent.Set();
		}

		public void Start(ManagerConfiguration managerConfiguration, IContainer container)
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<NodeManager>()
				 .As<INodeManager>();

			builder.RegisterType<JobManager>();

			builder.RegisterType<HttpSender>()
				 .As<IHttpSender>();

			builder.Register(
				 c => new JobRepository(managerConfiguration.ConnectionString))
				 .As<IJobRepository>();

			builder.Register(
				 c => new WorkerNodeRepository(managerConfiguration.ConnectionString))
				 .As<IWorkerNodeRepository>();

			builder.RegisterApiControllers(typeof(ManagerController).Assembly);

			builder.RegisterInstance(managerConfiguration);

			builder.Update(container);


			QuitEvent.WaitOne();

		}

	}

}