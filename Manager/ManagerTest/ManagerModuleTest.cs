using System.Configuration;
using Autofac;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

namespace ManagerTest
{
	[TestFixture]
	public class ManagerModuleTest
	{
		[SetUp]
		public void SetUp()
		{
			_containerBuilder = new ContainerBuilder();

			_containerBuilder.RegisterType<ManagerConfiguration>().SingleInstance();

			_containerBuilder.RegisterType<NodeManager>().As<INodeManager>();
			_containerBuilder.RegisterType<JobManagerNewVersion>().SingleInstance();
			_containerBuilder.RegisterType<Validator>().SingleInstance();
			_containerBuilder.RegisterType<HttpSender>().As<IHttpSender>();
			_containerBuilder.RegisterType<ManagerController>();

			_containerBuilder.Register(
				c => new JobRepositoryWithLock(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, new RetryPolicyProvider()))
				.As<IJobRepository>();

			_containerBuilder.Register(
				c => new WorkerNodeRepository(ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, new RetryPolicyProvider()))
				.As<IWorkerNodeRepository>();
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
		public void ShouldResolveNodeManager()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope())
				{
					scope.Resolve<INodeManager>();
					scope.Resolve<IHttpSender>();
					scope.Resolve<IJobRepository>();
					scope.Resolve<IWorkerNodeRepository>();

					ioc.IsRegistered<INodeManager>().Should().Be.True();
					ioc.IsRegistered<IHttpSender>().Should().Be.True();
					ioc.IsRegistered<IJobRepository>().Should().Be.True();
					ioc.IsRegistered<IWorkerNodeRepository>().Should().Be.True();
				}
			}
		}
	}
}