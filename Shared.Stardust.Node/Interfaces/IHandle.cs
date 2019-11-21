using System;
using System.Collections.Generic;
using System.Threading;

namespace Stardust.Node.Interfaces
{
	public interface IHandle<T>
	{
		void Handle(T parameters,
		            CancellationTokenSource cancellationTokenSource,
		            Action<string> sendProgress,
					ref IEnumerable<object> returnObjects);
	}
}