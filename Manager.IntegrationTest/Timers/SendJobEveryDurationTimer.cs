using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Manager.IntegrationTest.Timers
{
	public class SendJobEveryDurationTimer<T>
	{
		private int _currentIndex;

		public int TotalNumberOfTasks
		{
			get { return Tasks.Count; }
		}

		public List<Task<T>> Tasks { get; set; }

		public Action<int, int> ProgressAction { get; set; }

		public SendJobEveryDurationTimer(List<Task<T>>  tasks, 
														TimeSpan duration,
														Action<int,int> progressAction)
		{
			Tasks = tasks;
			ProgressAction = progressAction;

			SendTimer = new Timer
			{
				Interval = duration.TotalMilliseconds
			};

			SendTimer.Elapsed += SendTimer_Elapsed;
		}

		private void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Interlocked.Increment(ref _currentIndex);

			if (_currentIndex <= Tasks.Count)
			{
				Tasks[_currentIndex].Start();

				ProgressAction(_currentIndex, TotalNumberOfTasks);
			}
			else
			{
				SendTimer.Stop();
			}
		}

		public Timer SendTimer { get; private set; }
	}
}
