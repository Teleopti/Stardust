using System.Configuration;
using Autofac;
using ManagerTest.Fakes;
using Stardust.Manager;
using Stardust.Manager.Interfaces;

namespace ManagerTest
{
	public class JobTestsAttribute : BaseTestsAttribute
	{
		protected override void SetUp(ContainerBuilder builder)
		{
			FakeManagerConfiguration fakeManagerConfiguration = new FakeManagerConfiguration();
			builder.RegisterInstance(fakeManagerConfiguration).As<IManagerConfiguration>();
			builder.RegisterType<JobManager>();
			builder.RegisterType<NodeManager>();
			builder.Register(
				c => new JobRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString))
				.As<IJobRepository>();

			builder.Register(
				c => new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString))
				.As<IWorkerNodeRepository>();
			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance();
		}
	}
}