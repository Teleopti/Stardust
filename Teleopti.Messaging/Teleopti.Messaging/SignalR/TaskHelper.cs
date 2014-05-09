using System;
using System.Threading;
using System.Threading.Tasks;

namespace Teleopti.Messaging.SignalR
{
	public class TaskHelper
	{
		public static Task MakeEmptyTask()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetResult(null);
			return tcs.Task;
		}

		public static Task MakeDoneTask()
		{
			return MakeEmptyTask();
		}

		public static Task MakeFailedTask(Exception ex)
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(ex);
			return taskCompletionSource.Task;
		}

		public static Task Delay(TimeSpan timeOut)
		{
			var tcs = new TaskCompletionSource<object>();

			var timer = new Timer(tcs.SetResult,
			                      null,
			                      timeOut,
			                      TimeSpan.FromMilliseconds(-1));
			return tcs.Task.ContinueWith(_ => timer.Dispose(), TaskContinuationOptions.ExecuteSynchronously);
		}

	}
}