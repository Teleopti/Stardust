using System;
using System.Configuration;
using Autofac;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Interfaces;

namespace ManagerTest
{
	public class ManagerOperationTestsAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			FakeManagerConfiguration fakeManagerConfiguration = new FakeManagerConfiguration();
			builder.RegisterInstance(fakeManagerConfiguration).As<IManagerConfiguration>();

			builder.RegisterType<NodeManager>().As<INodeManager>().SingleInstance().AsSelf();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance().AsSelf();
			builder.Register(
				c => new JobRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString))
				.As<IJobRepository>();

			builder.Register(
				c => new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString))
				.As<IWorkerNodeRepository>();
			builder.RegisterType<ManagerController>();
			builder.RegisterType<JobManager>();
		}
		
	}
}