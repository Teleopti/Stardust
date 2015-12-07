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
	public class UseOnToggleEventHandlerTest
	{
		public IComponentContext Container;

		[Test]
		[Toggle(Toggles.TestToggle)]
		public void ShouldResolveHandlerUsedOnTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(HandlerUsedOnTestToggle));
		}

		[Test]
		[ToggleOff(Toggles.TestToggle)]
		public void ShouldNotResolveHandlerUsedOnTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(HandlerUsedOnTestToggle));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		[Toggle(Toggles.TestToggle2)]
		public void ShouldResolveHandlerWithMethodUsedOnTestToggle2()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggle2Event>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(HandlerUsedOnTestToggle_WithMethodUsedOnTestToggle2));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		[ToggleOff(Toggles.TestToggle2)]
		public void ShouldNotResolveHandlerWithMethodUsedOnTestToggle2()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggle2Event>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(HandlerUsedOnTestToggle_WithMethodUsedOnTestToggle2));
		}

		[Test]
		[Toggle(Toggles.TestToggle)]
		[ToggleOff(Toggles.TestToggle2)]
		public void ShouldResolveHandlerWithMethodUsedOnTestToggle2ByTestToggle()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggleEvent>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Contain(typeof(HandlerUsedOnTestToggle_WithMethodUsedOnTestToggle2));
		}

		[Test]
		[ToggleOff(Toggles.TestToggle)]
		[Toggle(Toggles.TestToggle2)]
		public void ShouldNotResolveHandlerUsedOnTestToggleWithMethodUsedOnTestToggle2()
		{
			var handlers = Container
				.Resolve<IEnumerable<IHandleEvent<TestToggle2Event>>>()
				.Select(ProxyUtil.GetUnproxiedType);

			handlers.Should()
				.Not.Contain(typeof(HandlerUsedOnTestToggle_WithMethodUsedOnTestToggle2));
		}


	}
}