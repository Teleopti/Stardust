using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Teleopti.Ccc.Web.Broker
{
	public class ActionThrottle : IDisposable, IActionScheduler
	{
		private readonly BlockingCollection<Action> actions = new BlockingCollection<Action>();
		private readonly int actionDelay;
		private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
		
		public readonly ILog Logger = LogManager.GetLogger(typeof(ActionThrottle));

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
		            while (!cancellation.Token.IsCancellationRequested)
		            {
		                Action item;
		                if (actions.TryTake(out item, -1, cancellation.Token))
		                {
		                    item();
		                    TaskEx.Delay(actionDelay, cancellation.Token).Wait(cancellation.Token);
		                }
		            }
		        },
		                          cancellation.Token,
		                          TaskCreationOptions.LongRunning,
		                          TaskScheduler.Default);
		    /*
		    Task task = null;
		    Task currentTask = null;
		    foreach (var action in actions.GetConsumingEnumerable(cancellation.Token))
		    {
		        var currentAction = action;
                if (currentTask==null)
                {
                    task = new Task(action);
                    currentTask = task;
                }
                else
                {
                    currentTask = currentTask.ContinueWith(_ => currentAction(), cancellation.Token);
                }
                currentTask.ContinueWith(t => Logger.Error(t.Exception.GetBaseException()), cancellation.Token,
		                                 TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
		        currentTask = currentTask.ContinueWith(_ => TaskEx.Delay(actionDelay, cancellation.Token));
		    }

            if (task!=null)
                task.Start();
             */
		}

		public void Dispose()
		{
			cancellation.Cancel();
		}
	}

    public static class TaskEx
    {
        static readonly Task _sPreCompletedTask = GetCompletedTask();
        static readonly Task _sPreCanceledTask = GetPreCanceledTask();

        public static Task Delay(int dueTimeMs, CancellationToken cancellationToken)
        {
            if (dueTimeMs < -1)
                throw new ArgumentOutOfRangeException("dueTimeMs", "Invalid due time");
            if (cancellationToken.IsCancellationRequested)
                return _sPreCanceledTask;
            if (dueTimeMs == 0)
                return _sPreCompletedTask;

            var tcs = new TaskCompletionSource<object>();
            var ctr = new CancellationTokenRegistration();
            var timer = new Timer(delegate(object self)
            {
                ctr.Dispose();
                ((Timer)self).Dispose();
                tcs.TrySetResult(null);
            });
            if (cancellationToken.CanBeCanceled)
                ctr = cancellationToken.Register(delegate
                {
                    timer.Dispose();
                    tcs.TrySetCanceled();
                });

            timer.Change(dueTimeMs, -1);
            return tcs.Task;
        }

        public static Task FinishedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }

        private static Task GetPreCanceledTask()
        {
            var source = new TaskCompletionSource<object>();
            source.TrySetCanceled();
            return source.Task;
        }

        private static Task GetCompletedTask()
        {
            var source = new TaskCompletionSource<object>();
            source.TrySetResult(null);
            return source.Task;
        }
    }
}