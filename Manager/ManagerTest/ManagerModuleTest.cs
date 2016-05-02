using System.Configuration;
using Autofac;
using ManagerTest.Database;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

namespace ManagerTest
{
	[TestFixture]
	public class ManagerModuleTest : DatabaseTest
	{
		[SetUp]
		public void SetUp()
		{
			_containerBuilder = new ContainerBuilder();

			_containerBuilder.RegisterType<NodeManager>().SingleInstance();
			_containerBuilder.RegisterType<JobManager>().SingleInstance();
			_containerBuilder.RegisterType<Validator>().SingleInstance();
			_containerBuilder.RegisterType<HttpSender>().As<IHttpSender>();
			_containerBuilder.RegisterType<ManagerController>();

			ManagerConfiguration config = new ManagerConfiguration()
			{
				ConnectionString = ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString
			};

			_containerBuilder.RegisterInstance(config).SingleInstance();
			_containerBuilder.RegisterType<RetryPolicyProvider>().SingleInstance();
			_containerBuilder.RegisterType<CreateSqlCommandHelper>().SingleInstance();
			_containerBuilder.RegisterType<JobRepository>().As<IJobRepository>().SingleInstance();
			_containerBuilder.RegisterType<WorkerNodeRepository>().As<IWorkerNodeRepository>().SingleInstance();

		}

		private ContainerBuilder _containerBuilder;

		[Test]
		public void ShouldResolveManagerController()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope())
				{
					var controller = scope.Resolve<ManagerController>();

					controller.Should().Not.Be.Null();
				}
			}
		}

		[Test]
		public void ShouldResolveStuff()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope())
				{
					scope.Resolve<IHttpSender>();
					scope.Resolve<IJobRepository>();
					scope.Resolve<IWorkerNodeRepository>();
					
					ioc.IsRegistered<IHttpSender>().Should().Be.True();
					ioc.IsRegistered<IJobRepository>().Should().Be.True();
					ioc.IsRegistered<IWorkerNodeRepository>().Should().Be.True();
				}
			}
		}
	}
}