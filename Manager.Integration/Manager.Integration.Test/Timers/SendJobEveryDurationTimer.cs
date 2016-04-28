using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Manager.Integration.Test.Timers
{
	public class SendJobEveryDurationTimer<T>
	{
		private int _currentIndex;

		public List<Task<T>> Tasks { get; set; }

		public SendJobEveryDurationTimer(List<Task<T>>  tasks, TimeSpan duration)
		{
			Tasks = tasks;

			SendTimer = new Timer
			{
				Interval = duration.TotalMilliseconds
			};

			SendTimer.Elapsed += SendTimer_Elapsed;
		}

		private void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Interlocked.Increment(ref _currentIndex);

			if (_currentIndex < Tasks.Count)
			{
				Tasks[_currentIndex].Start();
			}
			else
			{
				SendTimer.Stop();
			}
		}

		public Timer SendTimer { get; private set; }
	}
}
