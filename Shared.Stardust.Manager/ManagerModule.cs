using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Policies;
using Stardust.Manager.Timers;
using Stardust.Manager.Validations;

namespace Stardust.Manager
{
	public class ManagerModule : Module
	{
		private readonly ManagerConfiguration _managerConfiguration;

		public ManagerModule(ManagerConfiguration managerConfiguration)
		{
			_managerConfiguration = managerConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_managerConfiguration).As<ManagerConfiguration>().SingleInstance();
			builder.RegisterType<NodeManager>().SingleInstance();
            builder.RegisterType<JobManager>().As<IJobManager>().SingleInstance();
            builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<JobPurgeTimer>().SingleInstance();
			builder.RegisterType<NodePurgeTimer>().SingleInstance();
			builder.RegisterType<HalfNodesAffinityPolicy>().SingleInstance();
			builder.RegisterType<JobRepositoryCommandExecuter>().SingleInstance();
			builder.RegisterType<HttpSender>().As<IHttpSender>().SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			builder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();
            builder.RegisterApiControllers(typeof(ManagerController).Assembly);
            //builder.Register(c => LogManager.GetLogger(c.GetType()));
            builder.Register(c => LogManager.GetLogger("Stardust.ManagerLog"));
		}
	}
}
