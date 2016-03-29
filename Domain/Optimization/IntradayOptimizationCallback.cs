using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationCallback : IIntradayOptimizationCallback
	{
		private readonly ISchedulingProgress _backgroundWorker;
		private int _counter;
		private readonly string output = Resources.OptimizingIntraday + Resources.Colon + "({0}){1} {2} {3}";

		public IntradayOptimizationCallback(ISchedulingProgress backgroundWorker)
		{
			_backgroundWorker = backgroundWorker;
		}

		public void Optimizing(IntradayOptimizationCallbackInfo callbackInfo)
		{
			var e = new ResourceOptimizerProgressEventArgs(0, 0,
				string.Format(output, callbackInfo.NumberOfOptimizers, _counter++,
					callbackInfo.Agent.Name.ToString(NameOrderOption.FirstNameLastName),
					callbackInfo.WasSuccessful ? Resources.wasSuccessful : Resources.wasNotSuccessful));

			_backgroundWorker.ReportProgress(1, e);
		}

		public bool IsCancelled()
		{
			return _backgroundWorker.CancellationPending;
		}
	}
}