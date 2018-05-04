using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes.InvokeHandlers
{
	internal class LongRunningInvokeHandlerFake : IInvokeHandler
	{
		public void Invoke(object query,
			CancellationTokenSource cancellationTokenSource,
			Action<string> progressCallback,
			ref IEnumerable<object> environmentExitCode)
		{
			progressCallback.Invoke("Started working on longrunning stuff");
			Thread.Sleep(5000);
			cancellationTokenSource.Token.ThrowIfCancellationRequested();
		}
	}
}