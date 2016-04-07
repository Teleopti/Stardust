using System.Threading;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace NodeTest.Fakes.Timers
{
	public class TrySendJobProgressToManagerTimerFake : TrySendJobProgressToManagerTimer
	{
		public TrySendJobProgressToManagerTimerFake(NodeConfiguration nodeConfiguration, 
													IHttpSender httpSender,
		                                            double interval) : base(nodeConfiguration, 
																			httpSender, 
																			interval)
		{
			WaitHandle = new ManualResetEventSlim();
		}

		public ManualResetEventSlim WaitHandle;

		protected override void TrySendJobProgress()
		{
			WaitHandle.Set();
		}
	}
}