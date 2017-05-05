using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class ConfigurableSyncEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly ICurrentDataSource _dataSource;
		private readonly List<Type> _handlerTypes = new List<Type>();

		public ConfigurableSyncEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor, ICurrentDataSource dataSource)
		{
			_resolver = resolver;
			_processor = processor;
			_dataSource = dataSource;
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
			var tenant = _dataSource.CurrentName();
			onAnotherThread(() =>
			{
				foreach (var @event in events)
				{
					foreach (var handlerType in _handlerTypes)
					{
						var method = _resolver.HandleMethodFor(handlerType, @event.GetType());
						if (method == null)
							continue;
						_processor.Process(tenant, @event, null, handlerType);
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