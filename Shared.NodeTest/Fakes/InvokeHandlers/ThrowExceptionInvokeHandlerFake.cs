using System;
using System.Threading;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes.InvokeHandlers
{
	internal class ThrowExceptionInvokeHandlerFake : IInvokeHandler
	{
		public void Invoke(object query,
			CancellationTokenSource cancellationTokenSource,
			Action<string> progressCallback)
		{
			Thread.Sleep(TimeSpan.FromSeconds(2));

			throw new Exception();
		}
	}
}