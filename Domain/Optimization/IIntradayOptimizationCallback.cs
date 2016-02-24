using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizationCallback
	{
		void Optimizing(IntradayOptimizationCallbackInfo callbackInfo);
		bool IsCancelled();
	}

	public class IntradayOptimizationCallbackInfo
	{
		public IntradayOptimizationCallbackInfo(IPerson agent, bool wasSuccessful, int numberOfOptimizers, int executedOptimizers)
		{
			Agent = agent;
			WasSuccessful = wasSuccessful;
			NumberOfOptimizers = numberOfOptimizers;
			ExecutedOptimizers = executedOptimizers;
		}

		public IPerson Agent { get; private set; }
		public bool WasSuccessful { get; private set; }
		public int NumberOfOptimizers { get; private set; }
		public int ExecutedOptimizers { get; private set; }
	}
}