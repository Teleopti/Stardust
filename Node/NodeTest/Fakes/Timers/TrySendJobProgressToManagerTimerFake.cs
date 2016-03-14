using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
	public class TrySendJobProgressToManagerTimerFake : TrySendJobProgressToManagerTimer
	{
		public TrySendJobProgressToManagerTimerFake(INodeConfiguration nodeConfiguration, 
													IHttpSender httpSender,
		                                            double interval) : base(nodeConfiguration, 
																			httpSender, 
																			interval)
		{
		}

		protected override void TrySendJobProgress()
		{
			// Do nothing.
		}
	}
}