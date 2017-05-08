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

	[TestFixture]
	[IoCTest]
	public class EnabledOrDisabledByToggleEventHandlerTest
	{
		public IComponentContext Container;

		[Test]
		[Toggle(Toggles.TestToggle)]
		public void ShouldResolveHandlerEnabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(HandlerEnabledByTestToggle));
		}

		[Test]
		[ToggleOff(Toggles.TestToggle)]
		public void ShouldNotResolveHandlerEnabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(HandlerEnabledByTestToggle));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		[Toggle(Toggles.TestToggle2)]
		public void ShouldResolveHandlerWithMethodEnabledByTestToggle2()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggle2Event>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		[ToggleOff(Toggles.TestToggle2)]
		public void ShouldNotResolveHandlerWithMethodEnabledByTestToggle2()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggle2Event>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		[ToggleOff(Toggles.TestToggle2)]
		public void ShouldResolveHandlerWithMethodEnabledByTestToggle2ByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2));
		}

		[Test]
		[ToggleOff(Toggles.TestToggle)]
		[Toggle(Toggles.TestToggle2)]
		public void ShouldNotResolveHandlerEnabledByTestToggleWithMethodEnabledByTestToggle2()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggle2Event>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(HandlerEnabledByTestToggle_WithMethodEnabledByTestToggle2));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		public void ShouldNotResolveHandlerDisabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType).ToList();

			handlers.Should()
				.Not.Contain(typeof(HandlerDisabledByTestToggle));
			handlers.Should()
				.Contain(typeof(HandlerEnabledByTestToggle));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		public void ShouldNotResolveHandlerWithHandleDisabledByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(HandlerMethodDisabledByTestToggle));
		}
	}
}