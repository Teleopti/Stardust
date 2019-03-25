using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Node;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest
{
	[TestFixture]
	class NodeModuleTest
	{
		[SetUp]
		public void SetUp()
		{
			var nodeConfiguration = new NodeConfiguration(
				new Uri(ConfigurationManager.AppSettings["ManagerLocation"]),
				Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]),
				14100,
				"TestNode",
				60,
				2000,true);

			var builder = new ContainerBuilder();
            builder.RegisterInstance(new NodeConfigurationService());
			builder.RegisterModule(new NodeModule());

			_container = builder.Build();
		}

		private IContainer _container;

		[Test]
		public void ShouldResolveObjects()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.Resolve<NodeController>().Should().Not.Be.Null();
				scope.Resolve<NodeConfigurationService>().Should().Not.Be.Null();
				scope.Resolve<TrySendJobDetailToManagerTimer>().Should().Not.Be.Null();
				scope.Resolve<TrySendNodeStartUpNotificationToManagerTimer>().Should().Not.Be.Null();
				scope.Resolve<IPingToManagerTimer>().Should().Not.Be.Null();
				scope.Resolve<TrySendJobFaultedToManagerTimer>().Should().Not.Be.Null();
				scope.Resolve<TrySendJobCanceledToManagerTimer>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveInterfaces()
		{
			using (var scope = _container.BeginLifetimeScope())
			{
				scope.Resolve<IHttpSender>().Should().Not.Be.Null(); 
				scope.Resolve<IInvokeHandler>().Should().Not.Be.Null(); 
				scope.Resolve<IWorkerWrapper>().Should().Not.Be.Null(); 

				_container.IsRegistered<IHttpSender>().Should().Be.True();
				_container.IsRegistered<IInvokeHandler>().Should().Be.True();
				_container.IsRegistered<IWorkerWrapper>().Should().Be.True();
			}
		}
	}
}
