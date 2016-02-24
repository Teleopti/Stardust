using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationCallbackContext
	{
		[ThreadStatic]
		private static IIntradayOptimizationCallback _current;
		private static readonly IIntradayOptimizationCallback _default = new nullIntradayOptimizationCallback();

		public IDisposable Create(IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			_current = intradayOptimizationCallback;
			return new GenericDisposable(() => _current = null);
		}

		public IIntradayOptimizationCallback Current()
		{
			return _current ?? _default;
		}

		private class nullIntradayOptimizationCallback : IIntradayOptimizationCallback
		{
			public void Optimizing(IPerson agent, bool wasSuccessful, int numberOfOptimizers, int executedOptimizers)
			{
			}

			public bool IsCancelled()
			{
				return false;
			}
		}
	}
}