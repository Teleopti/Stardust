using System;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncInFatClientProcessEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly IEventRunner _eventRunner;

		public SyncInFatClientProcessEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor, IEventRunner eventRunner)
		{
			_resolver = resolver;
			_processor = processor;
			_eventRunner = eventRunner;
		}

		public void Publish(params IEvent[] events)
		{
			_eventRunner.RunEvents(_resolver, _processor, events);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
	public interface IEventRunner
	{
		void RunEvents(ResolveEventHandlers resolver, CommonEventProcessor processor, params IEvent[] events);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
	public class EventRunner : IEventRunner
	{
		public void RunEvents(ResolveEventHandlers resolver, CommonEventProcessor processor, params IEvent[] events)
		{
			Task.WaitAll((
					from @event in events
#pragma warning disable 618
					from handlerType in resolver.HandlerTypesFor<IRunInSyncInFatClientProcess>(@event)
					select Task.Run(() =>
							processor.Process(@event, handlerType)
#pragma warning restore 618
					))
				.ToArray());
		}
	}

	[RemoveMeWithToggle("temp, move back into SyncInFatClientProcessEventPublisher", Toggles.ResourcePlanner_LessResourcesXXL_74915)]
	public class EventRunnerThrottled : IEventRunner
	{
		public void RunEvents(ResolveEventHandlers resolver, CommonEventProcessor processor, params IEvent[] events)
		{
#pragma warning disable 618
			var eventInfos = from @event in events
				from handlerType in resolver.HandlerTypesFor<IRunInSyncInFatClientProcess>(@event)
				select new eventAndHandlerType { Event = @event, HandlerType = handlerType };

			Parallel.ForEach(eventInfos, 
			eventInfo =>
			{
				processor.Process(eventInfo.Event, eventInfo.HandlerType);
			});
#pragma warning restore 618
		}

		private class eventAndHandlerType
		{
			public IEvent @Event { get; set; }
			public Type HandlerType { get; set; }
		}
	}	
}
