using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationCallback : IIntradayOptimizationCallback
	{
		private readonly ISchedulingProgress _backgroundWorker;
		private int _counter;
		private readonly string output = Resources.OptimizingIntraday + Resources.Colon + "({0}){1} {2} {3}";
		private static readonly long waitBetweenCallbacks = TimeSpan.FromSeconds(0.2).Ticks;

		public IntradayOptimizationCallback(ISchedulingProgress backgroundWorker)
		{
			_backgroundWorker = backgroundWorker;
		}

		public void Optimizing(IntradayOptimizationCallbackInfo callbackInfo)
		{
			//this is here because of "half dead GUI" if many islands... Maybe overkill?
			if (preventCallbackDueToTooMany())
				return;

			var e = new ResourceOptimizerProgressEventArgs(0, 0,
				string.Format(output, callbackInfo.NumberOfOptimizers, _counter++, callbackInfo.Name,
					callbackInfo.WasSuccessful ? Resources.wasSuccessful : Resources.wasNotSuccessful), 100);

			_backgroundWorker.ReportProgress(1, e);
		}

		private long _lastUpdated = DateTime.UtcNow.Ticks;
		private readonly object locker = new object();
		private bool preventCallbackDueToTooMany()
		{
			lock (locker)
			{
				var ticksNow = DateTime.UtcNow.Ticks;
				if (ticksNow - waitBetweenCallbacks < _lastUpdated)
				{
					return true;
				}
				_lastUpdated = ticksNow;
				return false;
			}
		}

		public bool IsCancelled()
		{
			return _backgroundWorker.CancellationPending;
		}
	}
}