using System.Threading;
using System.Timers;
using Stardust.Node.Workers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendJobDetailToManagerTimer : Timer
	{
		private readonly JobDetailSender _jobDetailSender;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public TrySendJobDetailToManagerTimer(JobDetailSender jobDetailSender)
		{
			_jobDetailSender = jobDetailSender;
			_cancellationTokenSource = new CancellationTokenSource();
			Elapsed += OnTimedEvent;
		}

		public void SetupAndStart(NodeConfiguration nodeConfiguration)
		{
			Interval = nodeConfiguration.SendDetailsToManagerMilliseconds;
			base.Start();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_cancellationTokenSource != null &&
				!_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			Stop();
			try
			{
				_jobDetailSender.Send(_cancellationTokenSource.Token);
			}

			finally
			{
				Start();
			}
		}
	}
}