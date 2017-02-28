using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[LoggedOff]
	public class RtaTestAttribute : RtaTestLoggedOnAttribute
	{
	}

	public class RtaTestLoggedOnAttribute : DomainTestAttribute
	{
		public FakeEventPublisher_ExperimentalEventPublishing Publisher;
		public MutableNow_ExperimentalEventPublishing Now;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble<FakeEventPublisher_ExperimentalEventPublishing>().For<FakeEventPublisher, IEventPublisher>();
			system.UseTestDouble<MutableNow_ExperimentalEventPublishing>().For<MutableNow, INow>();
			system.UseTestDouble<FakeUnitOfWorkAspect_ExperimentalEventPublishing>().For<IUnitOfWorkAspect, IAllBusinessUnitsUnitOfWorkAspect>();

			// disable activity change checker triggered by minute tick which is triggered by Now.Is(...)
			system.UseTestDouble<DontCheckForActivityChangesFromScheduleChangeProcessor>().For<IActivityChangeCheckerFromScheduleChangeProcessor>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			Now.Is("2001-12-18 13:31");

			Publisher.AddHandler<Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();

			Publisher.AddHandler<ExternalLogonReadModelUpdater>();
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<ScheduleChangeProcessor>();
			Publisher.AddHandler<AgentStateMaintainer>();
		}
	}

	public class FakeUnitOfWorkAspect_ExperimentalEventPublishing : IUnitOfWorkAspect, IAllBusinessUnitsUnitOfWorkAspect
	{
		private readonly IEventPopulatingPublisher _publisher;
		private readonly FakeStorage _storage;
		private readonly IEnumerable<ITransactionHook> _hooks;
		private readonly INow _now;

		public FakeUnitOfWorkAspect_ExperimentalEventPublishing(
			IEventPopulatingPublisher publisher,
			FakeStorage storage,
			IEnumerable<ITransactionHook> hooks,
			INow now
			)
		{
			_publisher = publisher;
			_storage = storage;
			_hooks = hooks;
			_now = now;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
			var changes = _storage.Commit();
			if (changes.IsEmpty())
				return;

			Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} OnAfterInvocation {changes.Count()}");
			changes.ForEach(x => Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {x.Root.GetType()} {x.Status}"));

			// required for rta tests, really, PA is missing events...
			new ScheduleChangedEventPublisher(_publisher, _now).AfterCompletion(changes);
			_hooks.ForEach(x => x.AfterCompletion(changes));
			_publisher.Publish(new TenantMinuteTickEvent());
		}
	}

	public class MutableNow_ExperimentalEventPublishing : MutableNow
	{
		private readonly IEventPublisher _publisher;

		public MutableNow_ExperimentalEventPublishing(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public override void Is(DateTime? utc)
		{
			var dayTick = Math.Abs(utc.GetValueOrDefault().Subtract(UtcDateTime()).TotalDays) >= 1;
			var hourTick = Math.Abs(utc.GetValueOrDefault().Subtract(UtcDateTime()).TotalHours) >= 1;

			base.Is(utc);

			Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {utc}");

			if (dayTick)
				_publisher.Publish(new TenantDayTickEvent());
			if (hourTick)
				_publisher.Publish(new TenantHourTickEvent());
			_publisher.Publish(new TenantMinuteTickEvent());
		}
	}

	public class FakeEventPublisher_ExperimentalEventPublishing : FakeEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly List<Type> _handlerTypes = new List<Type>();

		public FakeEventPublisher_ExperimentalEventPublishing(ResolveEventHandlers resolver, CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void AddHandler<T>()
		{
			_handlerTypes.Add(typeof(T));
		}

		public void AddHandler(Type type)
		{
			_handlerTypes.Add(type);
		}

		public override void Publish(params IEvent[] events)
		{
			base.Publish(events);

			if (PublishedEvents.Count() > 100)
				throw new Exception("Looks like a circular/recursive event chain?");

			foreach (var @event in events)
			{
				foreach (var handlerType in _handlerTypes)
				{
					var method = _resolver.HandleMethodFor(handlerType, @event);
					if (method == null)
						continue;

					events.ForEach(e => Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {e} to {handlerType}"));

					onAnotherThread(() =>
					{
						_processor.Process(@event, handlerType);
					});
				}
			}
		}

		private static void onAnotherThread(Action action)
		{
			var thread = new Thread(action.Invoke);
			thread.Start();
			thread.Join();
		}
	}
}
