using System.Threading;
using Stardust.Node;
using Stardust.Node.Interfaces;
using Stardust.Node.Timers;

namespace NodeTest.Fakes.Timers
{
	public class TrySendJobDetailToManagerTimerFake : TrySendJobDetailToManagerTimer
	{
		public TrySendJobDetailToManagerTimerFake(NodeConfiguration nodeConfiguration, 
													IHttpSender httpSender) : base(nodeConfiguration, 
																			httpSender)
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