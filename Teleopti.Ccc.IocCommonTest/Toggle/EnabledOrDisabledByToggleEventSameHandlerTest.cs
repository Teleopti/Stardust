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
	public class EnabledOrDisabledByToggleEventSameHandlerTest
	{
		public IComponentContext Container;

		[Test]
		[Toggle(Toggles.TestToggle)]
		public void ShouldResolveByPackageInterfaceWhenEnabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvents>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(SameHandlerEnabledByTestToggle));
		}
		
		[Test]
		[ToggleOff(Toggles.TestToggle)]
		public void ShouldNotResolveByPackageInterfaceWhenDisabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvents>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(SameHandlerEnabledByTestToggle));
		}
		
		[Test]
		[ToggleOff(Toggles.TestToggle)]
		public void ShouldResolveBySingleInterfaceWhenDisabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(SameHandlerEnabledByTestToggle));
		}
		
		[Test]
		[Toggle(Toggles.TestToggle)]
		public void ShouldNotResolveBySingleInterfaceWhenEnabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(SameHandlerEnabledByTestToggle));
		}
	}
}