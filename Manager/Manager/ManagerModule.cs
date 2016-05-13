using System.Timers;
using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
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
			builder.RegisterType<JobManager>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();
			builder.RegisterType<PurgeTimer>().As<Timer>().SingleInstance();
			builder.RegisterType<CreateSqlCommandHelper>().SingleInstance();
			builder.RegisterType<HttpSender>().As<IHttpSender>().SingleInstance();
			builder.RegisterType<RetryPolicyProvider>().SingleInstance();
			builder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			builder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();

			builder.RegisterApiControllers(typeof(ManagerController).Assembly);
		}
	}
}
