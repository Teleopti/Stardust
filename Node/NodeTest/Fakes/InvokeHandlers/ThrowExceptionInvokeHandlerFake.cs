using System;
using System.Collections.Generic;
using System.Threading;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes.InvokeHandlers
{
	internal class ThrowExceptionInvokeHandlerFake : IInvokeHandler
	{
		public void Invoke(object query,
			CancellationTokenSource cancellationTokenSource,
			Action<string> progressCallback,
			ref IEnumerable<object> returnObjects)
		{
			Thread.Sleep(TimeSpan.FromSeconds(2));

			throw new Exception();
		}
	}
}