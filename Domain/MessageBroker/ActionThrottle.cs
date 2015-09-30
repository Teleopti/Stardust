using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public class ActionThrottle : IDisposable, IActionScheduler
	{
		private readonly BlockingCollection<Action> actions = new BlockingCollection<Action>();
		private readonly int actionDelay;
		private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
		public ILog Logger = LogManager.GetLogger(typeof(ActionThrottle));

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
			Task.Factory.StartNew(async () =>
			{
				Started();
				foreach (var action in actions.GetConsumingEnumerable(cancellation.Token))
				{
					try
					{
						action();
					}
					catch (Exception ex)
					{
						Logger.Error(ex);
					}
					await WaitForNext(actionDelay);
				}
			});
		}

		protected virtual void Started()
		{

		}

		protected virtual Task WaitForNext(int waitMilliseconds)
		{
			return Task.Delay(waitMilliseconds, cancellation.Token);
		}

		public virtual void Dispose()
		{
			cancellation.Cancel();
		}
	}

}