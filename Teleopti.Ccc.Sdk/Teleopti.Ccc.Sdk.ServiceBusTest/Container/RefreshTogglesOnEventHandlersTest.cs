using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Container
{
	[DomainTest]
	public class RefreshTogglesOnEventHandlersTest : IExtendSystem, IIsolateSystem
	{
		public ToggleFiller Filler;
		public ILifetimeScope Container;

		public static bool EventHandlerWasCalled;
		private IEnumerable<object> dummy;
		
		[Test]
		public void ShouldRefreshTogglesWhenStardustEventHandler()
		{
			Container.Resolve<IHandle<StardustEvent>>().Handle(null, null, null, ref dummy);
			Filler.WasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldRefreshTogglesForEachResolve()
		{
			Container.Resolve<IHandle<StardustEvent>>().Handle(null, null, null, ref dummy);
			Container.Resolve<IHandle<StardustEvent>>().Handle(null, null, null, ref dummy);
			Container.Resolve<IHandle<StardustEvent>>().Handle(null, null, null, ref dummy);
			Filler.Called.Should().Be.EqualTo(3);
		}
		
		[Test]
		public void ShouldCallInnerService()
		{
			EventHandlerWasCalled = false;
			var svc = Container.Resolve<IHandle<StardustEvent>>();
			svc.Handle(null, null, null, ref dummy);
			EventHandlerWasCalled.Should().Be.True();
		}

		[Test]
		public void ShouldOnlyRegisterOneType()
		{
			Container.Resolve<IEnumerable<IHandle<StardustEvent>>>()
				.Count().Should().Be.EqualTo(1);
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new NodeHandlersModule(configuration));
			extend.AddService<StardustEventHandler, IHandle<StardustEvent>>(NodeHandlersModule.NodeComponentName);
		}

		public class StardustEventHandler : IHandle<StardustEvent>
		{
			public void Handle(StardustEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress,
				ref IEnumerable<object> returnObjects)
			{
				EventHandlerWasCalled = true;
			}
		}
		public class StardustEvent{}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ToggleFiller>().For<IToggleFiller, ToggleFiller>();
		}
		
		public class ToggleFiller : IToggleFiller
		{
			public void RefetchToggles()
			{
				Called++;
			}

			public bool WasCalled => Called > 0;
			public int Called { get; private set; }
		}
	}
}