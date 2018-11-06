﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisher : IEventPublisher
	{
		private readonly IRtaEventPublisher _rtaPublisher;
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;
		private readonly ICurrentDataSource _dataSource;
		private readonly IThreadPrincipalContext _threadPrincipalContext;
		private readonly List<Type> _handlerTypes = new List<Type>();
		private ConcurrentQueue<IEvent> queuedEvents = new ConcurrentQueue<IEvent>();

		public FakeEventPublisher(
			IRtaEventPublisher rtaPublisher,
			ResolveEventHandlers resolver,
			CommonEventProcessor processor,
			ICurrentDataSource dataSource,
			IThreadPrincipalContext threadPrincipalContext)
		{
			_rtaPublisher = rtaPublisher;
			_resolver = resolver;
			_processor = processor;
			_dataSource = dataSource;
			_threadPrincipalContext = threadPrincipalContext;
		}

		public IEnumerable<IEvent> PublishedEvents => queuedEvents.ToArray();

		public void Clear()
		{
			queuedEvents = new ConcurrentQueue<IEvent>();
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
			_rtaPublisher.Publish(events);

			events.ForEach(queuedEvents.Enqueue);

			if (_handlerTypes.IsEmpty())
				return;

			var tenant = _dataSource.CurrentName();

			onAnotherThread(() =>
			{
				_resolver.ResolveAllJobs(events)
					.ForEach(job =>
					{
						using (clearCurrentPrincipal())
						{
							if (_handlerTypes.Contains(job.HandlerType))
								_processor.Process(tenant, job.Event, job.Package, job.HandlerType);								
						}
					});
			});
		}

		private static void onAnotherThread(Action action)
		{
			var thread = new Thread(action.Invoke);
			thread.Start();
			thread.Join();
		}

		private IDisposable clearCurrentPrincipal()
		{
			var current = _threadPrincipalContext.Current();
			_threadPrincipalContext.SetCurrentPrincipal(null);
			return new GenericDisposable(() => _threadPrincipalContext.SetCurrentPrincipal(current));
		}
	}
}