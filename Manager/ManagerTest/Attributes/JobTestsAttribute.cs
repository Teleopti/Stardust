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
			builder.RegisterType<ManagerConfiguration>().SingleInstance();

			// This MUST be singleton.
			builder.RegisterType<JobManager>().SingleInstance();

			builder.RegisterType<NodeManager>();

			builder.Register(c => new JobRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, new RetryPolicyProvider()))
							.As<IJobRepository>();

			builder.Register(c => new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString,new RetryPolicyProvider()))
							.As<IWorkerNodeRepository>();

			builder.RegisterType<FakeHttpSender>().As<IHttpSender>().SingleInstance();
		}
	}
}