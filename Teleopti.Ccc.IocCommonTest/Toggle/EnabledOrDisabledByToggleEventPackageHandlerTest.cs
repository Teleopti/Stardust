using System.Collections.Generic;
using System.Linq;
using Autofac;
using Castle.DynamicProxy;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	[TestFixture]
	[IoCTest]
	public class EnabledOrDisabledByToggleEventPackageHandlerTest
	{
		public IComponentContext Container;

		[Test]
		[Toggle(Toggles.TestToggle)]
		public void ShouldResolveHandlerEnabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvents>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(PackageHandlerEnabledByTestToggle));
		}
	}
}