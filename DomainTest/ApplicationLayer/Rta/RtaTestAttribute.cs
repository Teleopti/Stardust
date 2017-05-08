using System;
using System.Collections.Generic;
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
		public FakeEventPublisher Publisher;
		public MutableNow_ExperimentalEventPublishing Now;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

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

			//Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} OnAfterInvocation {changes.Count()}");
			//changes.ForEach(x => Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {x.Root.GetType()} {x.Status}"));

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
			var time = new TimePassingSimulator(UtcDateTime(), utc.GetValueOrDefault());
			base.Is(utc);

			//Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {utc}");

			time.IfDayPassed(() => { _publisher.Publish(new TenantDayTickEvent()); });
			time.IfHourPassed(() => { _publisher.Publish(new TenantHourTickEvent()); });
			time.IfMinutePassed(() => { _publisher.Publish(new TenantMinuteTickEvent()); });
		}
	}
	
}
