using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	[TestFixture]
	public class BootstrapperModuleTest
	{
		private IContainerConfiguration containerConfiguration;
		
		[SetUp]
		public void Setup()
		{
			containerConfiguration = new ContainerConfiguration();
		}

		[Test]
		public void RegisteredGlobalFiltersShouldBeenRegistered()
		{
			using (var container = containerConfiguration.Configure(string.Empty))
			{
				var tasks = container.Resolve<IEnumerable<IBootstrapperTask>>();
				tasks.Should().Have.Count.GreaterThan(0);
			}
		}

		[Test]
		public void TasksShouldBeRegisteredAsSingletons()
		{
			using (var container = containerConfiguration.Configure(string.Empty))
			{
				var tasks = container.Resolve<IEnumerable<IBootstrapperTask>>();
				var tasks2 = container.Resolve<IEnumerable<IBootstrapperTask>>();
				tasks.Should().Have.SameValuesAs(tasks2);
			}
		}
	}
}
