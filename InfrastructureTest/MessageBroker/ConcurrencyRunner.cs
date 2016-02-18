using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class ConcurrencyRunner
	{
		private readonly List<Task> _tasks = new List<Task>();
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
			var task = Task.Factory.StartNew(action.Invoke);
			_tasks.Add(task);
		}

		public void Wait()
		{
			Task.WaitAll(_tasks.ToArray());
		}

		public void WaitForException<T>() where T : Exception
		{
			try
			{
				Task.WaitAll(_tasks.ToArray());
			}
			catch (Exception e)
			{
				var matched = e.AllExceptions().OfType<T>().FirstOrDefault();
				if (matched != null)
					throw matched;
				throw;
			}
		}

	}
}