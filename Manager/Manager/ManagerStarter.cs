﻿using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
		public void Start(ManagerConfiguration managerConfiguration, IComponentContext componentContext)
		{
			var builder = new ContainerBuilder();

			builder.RegisterInstance(managerConfiguration);

			builder.RegisterType<NodeManager>().SingleInstance();
			builder.RegisterType<JobManager>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();

			builder.RegisterType<HttpSender>().As<IHttpSender>();

			builder.Register(c => new JobRepository(managerConfiguration.ConnectionString,
															new RetryPolicyProvider()))
				.As<IJobRepository>();

			builder.Register(c => new WorkerNodeRepository(managerConfiguration.ConnectionString,
			                                               new RetryPolicyProvider()))
				.As<IWorkerNodeRepository>();

			builder.RegisterApiControllers(typeof (ManagerController).Assembly);

			builder.Update(componentContext.ComponentRegistry);
		}
	}
}