using System.Timers;
using Stardust.Node.Workers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendJobDetailToManagerTimer : Timer
	{
		private readonly JobDetailSender _jobDetailSender;

		public TrySendJobDetailToManagerTimer(NodeConfiguration nodeConfiguration, JobDetailSender jobDetailSender)
		{
			_jobDetailSender = jobDetailSender;
			Interval = nodeConfiguration.SendDetailsToManagerMilliSeconds;
			Elapsed += OnTimedEvent;
		}
		
		
		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			Stop();
			try
			{
				_jobDetailSender.Send();
			}

			finally
			{
				Start();
			}
		}
	}
}