using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class ConcurrencyRunner
	{
		private readonly List<Task> _tasks = new List<Task>();
		private readonly ConcurrentBag<Exception> _exceptions = new ConcurrentBag<Exception>();
		private Action _lastSyncAction;
		private Action _lastAsyncAction;

		public ConcurrencyRunner InSync(Action action)
		{
			_lastSyncAction = action;
			_lastAsyncAction = null;
			action.Invoke();
			return this;
		}

		public ConcurrencyRunner InParallel(Action action)
		{
			_lastSyncAction = null;
			_lastAsyncAction = action;
			addTask(_lastAsyncAction);
			return this;
		}

		public ConcurrencyRunner Times(int times)
		{
			if (_lastAsyncAction != null)
				(times - 1).Times(() => addTask(_lastAsyncAction));
			if (_lastSyncAction != null)
				(times - 1).Times(_lastSyncAction);
			return this;
		}

		private void addTask(Action action)
		{
			var task = Task.Factory.StartNew(() =>
			{
				try
				{
					action.Invoke();
				}
				catch (Exception e)
				{
					_exceptions.Add(e);
					throw;
				}
			});
			_tasks.Add(task);
		}

		public void WaitAll()
		{
			Task.WaitAll(_tasks.ToArray());
		}

		public void ThrowAnyException()
		{
			var exception = _exceptions.FirstOrDefault();
			if (exception == null)
				return;
			if (exception is AggregateException)
				exception = (exception as AggregateException).InnerException;
			throw exception;
		}
	}
}