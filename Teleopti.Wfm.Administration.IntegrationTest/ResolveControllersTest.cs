using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	[TestFixture]
	public class ResolveControllersTest : IocRegistrationTest
	{
		[Test]
		public void ResolveControllers([Values("Customer", "RC")] string toggleMode)
		{
			var container = InitContainer(toggleMode);
			var controllers = container.ComponentRegistry.Registrations
				.Where(r => r.Activator.LimitType.IsSubclassOf(typeof(ApiController)))
				.Select(x => x.Activator.LimitType.BaseType);

			foreach (var controller in controllers)
			{
				container.Resolve(controller);
			}
		}

		protected override IEnumerable<IModule> DefineModules(IocConfiguration iocConfiguration)
		{
			return new List<IModule> {new WfmAdminAppModule()};
		}
	}
}