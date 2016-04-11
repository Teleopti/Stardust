using System;
using System.Configuration;
using Autofac;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

namespace ManagerTest
{
	public class ManagerOperationTestsAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			builder.RegisterType<ManagerConfiguration>().SingleInstance();
			builder.RegisterType<Validator>().SingleInstance();

			builder.RegisterType<NodeManager>().As<INodeManager>().SingleInstance().AsSelf();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();
			builder.Register(
				c => new JobRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, new RetryPolicyProvider()))
				.As<IJobRepository>();

			builder.Register(
				c => new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,new RetryPolicyProvider()))
				.As<IWorkerNodeRepository>();
			builder.RegisterType<ManagerController>();

			builder.RegisterType<JobManager>().SingleInstance();
		}
	}
}