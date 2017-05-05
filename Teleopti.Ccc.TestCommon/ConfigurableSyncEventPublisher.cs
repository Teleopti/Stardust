using System;
using System.Collections.Generic;
using System.Threading;
using NHibernate.Util;
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
				_resolver.ResolveAllJobs(events)
					.ForEach(job =>
					{
						if (_handlerTypes.Contains(job.HandlerType))
							_processor.Process(tenant, job.Event, job.Package, job.HandlerType);
					});
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