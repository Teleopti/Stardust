using System.Configuration;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Timers;
using Stardust.Manager.Validations;

namespace ManagerTest
{
	[TestFixture]
	public class ManagerModuleTest
	{
		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ManagerConfiguration config = new ManagerConfiguration(
				ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString, "Route", 60, 20, 1, 1, 1,1);

			containerBuilder.RegisterModule(new ManagerModule(config));
			_container = containerBuilder.Build();
		}

		private IContainer _container;

		[Test]
		public void ShouldResolveClasses()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.Resolve<ManagerController>().Should().Not.Be.Null();
				scope.Resolve<ManagerConfiguration>().Should().Not.Be.Null();
				scope.Resolve<NodeManager>().Should().Not.Be.Null();
				scope.Resolve<IJobManager>().Should().Not.Be.Null();
				scope.Resolve<Validator>().Should().Not.Be.Null();
				scope.Resolve<RetryPolicyProvider>().Should().Not.Be.Null();
				scope.Resolve<JobPurgeTimer>().Should().Not.Be.Null();
				scope.Resolve<NodePurgeTimer>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveInterfaces()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.Resolve<IHttpSender>().Should().Not.Be.Null();
				scope.Resolve<IJobRepository>().Should().Not.Be.Null();
				scope.Resolve<IWorkerNodeRepository>().Should().Not.Be.Null();

				_container.IsRegistered<IHttpSender>().Should().Be.True();
				_container.IsRegistered<IJobRepository>().Should().Be.True();
				_container.IsRegistered<IWorkerNodeRepository>().Should().Be.True();
			}
		}
	}
}