using System.Collections.Generic;
using System.Web;
using System.Web.Http;
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
		[Test]
		public void RegisteredGlobalFiltersShouldBeenRegistered()
		{
			HttpContext.Current = null;
			var containerConfiguration = new ContainerConfiguration();
			using (var container = containerConfiguration.Configure(string.Empty, new HttpConfiguration()))
			{
				var tasks = container.Resolve<IEnumerable<IBootstrapperTask>>();
				tasks.Should().Have.Count.GreaterThan(0);
			}
		}

		[Test]
		public void TasksShouldBeRegisteredAsSingletons()
		{
			HttpContext.Current = null;
			var containerConfiguration = new ContainerConfiguration();
			using (var container = containerConfiguration.Configure(string.Empty, new HttpConfiguration()))
			{
				var tasks = container.Resolve<IEnumerable<IBootstrapperTask>>();
				var tasks2 = container.Resolve<IEnumerable<IBootstrapperTask>>();
				tasks.Should().Have.SameValuesAs(tasks2);
			}
		}
	}
}