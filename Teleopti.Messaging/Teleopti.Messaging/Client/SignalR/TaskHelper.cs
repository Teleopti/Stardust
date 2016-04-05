using System;
using System.Threading.Tasks;

namespace Teleopti.Messaging.Client.SignalR
{
	public class TaskHelper
	{
		public static Task MakeDoneTask()
		{
			return Task.FromResult(false);
		}

		public static Task MakeFailedTask(Exception ex)
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(ex);
			return taskCompletionSource.Task;
		}
	}
}