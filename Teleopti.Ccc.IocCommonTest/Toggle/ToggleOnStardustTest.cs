using System;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	[DomainTest]
	[Ignore("#79699 To be fixed")]
	public class ToggleOnStardustTest : IExtendSystem, IIsolateSystem
	{
		public ToggleFiller Filler;
		public ILifetimeScope Container;
		
		[TestCase(typeof(stardustEventHandler))]
		[TestCase(typeof(stardustEventHandler_Singleton))]
		public void ShouldRefreshTogglesBeforeCreatingStardustEventHandler(Type eventType)
		{
			Container.Resolve(eventType);
			Filler.WasCalled.Should().Be.True();
		}

		[TestCase(typeof(stardustEventHandler))]
		[TestCase(typeof(stardustEventHandler_Singleton))]
		public void ShouldRefreshTogglesForEachResolve(Type eventType)
		{
			((IHandleEvent<IEvent>)Container.Resolve(eventType)).Handle(null);
			((IHandleEvent<IEvent>)Container.Resolve(eventType)).Handle(null);
			((IHandleEvent<IEvent>)Container.Resolve(eventType)).Handle(null);
			Filler.Called.Should().Be.EqualTo(3);
		}
		
		[TestCase(typeof(nonStardustEventHandler))]
		[TestCase(typeof(nonStardustEventHandler_Singleton))]
		public void ShouldNotRefreshTogglesIfNonStardustEventHandler(Type eventType)
		{
			((IHandleEvent<IEvent>)Container.Resolve(eventType)).Handle(null);
			Filler.WasCalled.Should().Be.False();
		}

		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new eventHandlerModule());
		}

		[InstancePerLifetimeScope]
		public class stardustEventHandler : IRunOnStardust, IHandleEvent<IEvent>
		{
			public void Handle(IEvent @event)
			{
			}
		}
		
		[InstancePerLifetimeScope]
		public class nonStardustEventHandler : IRunOnHangfire, IHandleEvent<IEvent>
		{
			public void Handle(IEvent @event)
			{
			}
		}
		
		public class stardustEventHandler_Singleton : IRunOnStardust, IHandleEvent<IEvent>
		{
			public void Handle(IEvent @event)
			{
			}
		}
		
		public class nonStardustEventHandler_Singleton : IRunOnHangfire, IHandleEvent<IEvent>
		{
			public void Handle(IEvent @event)
			{
			}
		}

		public class eventHandlerModule : Module
		{
			protected override void Load(ContainerBuilder builder)
			{
				builder.RegisterEventHandlers((x) => { return true;}, GetType().Assembly);
			}
		}

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