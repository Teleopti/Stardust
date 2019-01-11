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
	public class RefreshTogglesOnEventHandlersTest : IExtendSystem, IIsolateSystem
	{
		public ToggleFiller Filler;
		public ILifetimeScope Container;
		
		[TestCase(typeof(IHandleEvent<IStardustSingletonEvent>))]
		[TestCase(typeof(IHandleEvent<IStardustEvent>))]
		public void ShouldRefreshTogglesWhenStardustEventHandler(Type eventType)
		{
			((dynamic)Container.Resolve(eventType)).Handle(null);
			Filler.WasCalled.Should().Be.True();
		}

		[TestCase(typeof(IHandleEvent<IStardustSingletonEvent>))]
		[TestCase(typeof(IHandleEvent<IStardustEvent>))]
		public void ShouldRefreshTogglesForEachResolve(Type eventType)
		{
			((dynamic)Container.Resolve(eventType)).Handle(null);
			((dynamic)Container.Resolve(eventType)).Handle(null);
			((dynamic)Container.Resolve(eventType)).Handle(null);
			Filler.Called.Should().Be.EqualTo(3);
		}
		
		[TestCase(typeof(IHandleEvent<INonStardustSingletonEvent>))]
		[TestCase(typeof(IHandleEvent<INonStardustEvent>))]
		public void ShouldNotRefreshTogglesIfNonStardustEventHandler(Type eventType)
		{
			((dynamic)Container.Resolve(eventType)).Handle(null);
			Filler.WasCalled.Should().Be.False();
		}

		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new EventHandlerModule());
		}

		[InstancePerLifetimeScope]
		public class StardustEventHandler : IRunOnStardust, IHandleEvent<IStardustEvent>
		{
			public virtual void Handle(IStardustEvent @event)
			{
			}
		}
		public interface IStardustEvent : IEvent{}
		
		[InstancePerLifetimeScope]
		public class NonStardustEventHandler : IRunOnHangfire, IHandleEvent<INonStardustEvent>
		{
			public virtual void Handle(INonStardustEvent @event)
			{
			}
		}
		public interface INonStardustEvent : IEvent{}
		
		public class StardustEventHandlerSingleton : IRunOnStardust, IHandleEvent<IStardustSingletonEvent>
		{
			public virtual void Handle(IStardustSingletonEvent @event)
			{
			}
		}
		public interface IStardustSingletonEvent : IEvent{}
		
		public class NonStardustEventHandlerSingleton : IRunOnHangfire, IHandleEvent<INonStardustSingletonEvent>
		{
			public virtual void Handle(INonStardustSingletonEvent @event)
			{
			}
		}
		public interface INonStardustSingletonEvent : IEvent{}

		public class EventHandlerModule : Module
		{
			protected override void Load(ContainerBuilder builder)
			{
				builder.RegisterEventHandlers((x) => true, GetType().Assembly);
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