using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	public class ConfigurableAsyncEventPublisher : IEventPublisher
	{
		private readonly ServiceBusEventProcessor _busProcessor;
		private readonly ResolveEventHandlers _resolver;
		private readonly List<Type> _handlerTypes = new List<Type>();
		 
		private readonly Queue<Thread> _threads = new Queue<Thread>();
		private readonly ConcurrentBag<Exception> _exceptions = new ConcurrentBag<Exception>();

		public ConfigurableAsyncEventPublisher(ServiceBusEventProcessor busProcessor, ResolveEventHandlers resolver)
		{
			_busProcessor = busProcessor;
			_resolver = resolver;
		}

		public void AddHandler(Type type)
		{
			_handlerTypes.Add(type);
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				foreach (var handlerType in _handlerTypes)
				{
					var method = _resolver.HandleMethodFor(handlerType, @event);
					if (method == null)
						continue;
					var thread = new Thread(() =>
					{
						try
						{
							var retries = 3;
							while (true)
							{
								try
								{
									_busProcessor.Process(@event, new[] { handlerType });
									Debug.WriteLine("done " + retries + " " + @event.GetType().Name + "-> " + handlerType.Name);
									break;
								}
								catch (Exception)
								{
									Debug.WriteLine("retry " + retries + " " + @event.GetType().Name + "-> " + handlerType.Name);
									retries--;
									if (retries == 0)
										throw;
									Thread.Sleep(new Random().Next(10, 1000));
								}
							}
						}
						catch (Exception e)
						{
							_exceptions.Add(e);
						}
					});
					thread.Start();
					_threads.Enqueue(thread);
				}
			}
		}

		public void Wait()
		{
			while (_threads.Count > 0)
				_threads.Dequeue().Join();
			if (_exceptions.Count > 0)
				throw new AggregateException(_exceptions);
		}
	}
}