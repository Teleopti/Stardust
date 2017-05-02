using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class ConfigurableAsyncEventPublisher : IEventPublisher
	{
		private readonly ServiceBusEventProcessor _busProcessor;
		private readonly ResolveEventHandlers _resolver;
		private readonly List<Type> _handlerTypes = new List<Type>();
		 
		private readonly ConcurrentQueue<Thread> _threads = new ConcurrentQueue<Thread>();
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
					var method = _resolver.HandleMethodFor(handlerType, @event.GetType());
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
									//Debug.WriteLine("done " + retries + " " + @event.GetType().Name + "-> " + handlerType.Name);
									break;
								}
								catch (Exception)
								{
									//Debug.WriteLine("retry " + retries + " " + @event.GetType().Name + "-> " + handlerType.Name);
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
			{
				var t = _threads;
				Thread a;
				t.TryDequeue(out a);
				a.Join();
			}
			if (_exceptions.Count > 0)
				throw new AggregateException(_exceptions);
		}
	}
}