using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class ConfigurableSyncEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly List<Type> _handlerTypes = new List<Type>();

		public ConfigurableSyncEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor)
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

		public void Publish(params IEvent[] events)
		{
			onAnotherThread(() =>
			{
				foreach (var @event in events)
				{
					foreach (var handlerType in _handlerTypes)
					{
						var method = _resolver.HandleMethodFor(handlerType, @event);
						if (method == null)
							continue;
						_processor.Process(@event, handlerType);
					}
				}
			});
		}

		private static void onAnotherThread(Action action)
		{
			var thread = new Thread(action.Invoke);
			thread.Start();
			thread.Join();
		}

	}
}