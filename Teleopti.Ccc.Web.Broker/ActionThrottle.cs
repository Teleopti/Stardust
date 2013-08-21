using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Web.Broker
{
	public class ActionThrottle : IDisposable, IActionScheduler
	{
		private readonly BlockingCollection<Action> actions = new BlockingCollection<Action>();
		private readonly int actionDelay;
		private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

		public ActionThrottle(int actionsPerSecond)
		{
			actionDelay = 1000 / actionsPerSecond;
		}

		public void Do(Action action)
		{
			actions.Add(action);
		}

		public void Start()
		{
			Task.Factory.StartNew(() =>
				{
					foreach (var action in actions.GetConsumingEnumerable(cancellation.Token))
					{
						action();
						Thread.Sleep(actionDelay);
					}
				});
		}

		public void Dispose()
		{
			cancellation.Cancel();
		}
	}
}