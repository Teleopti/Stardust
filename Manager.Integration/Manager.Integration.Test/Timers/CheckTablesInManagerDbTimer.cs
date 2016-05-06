using System;
using System.Collections.ObjectModel;
using System.Timers;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Models;

namespace Manager.Integration.Test.Timers
{
	public class CheckTablesInManagerDbTimer : IDisposable
	{
		public EventHandler<ObservableCollection<JobQueueItem>> ReceivedJobQueueItem;

		public EventHandler<ObservableCollection<Job>> ReceivedJobItem;

		public EventHandler<ObservableCollection<JobDetail>> ReceivedJobDetailItem;

		public EventHandler<ObservableCollection<Logging>> ReceivedLoggingData;

		public EventHandler<ObservableCollection<WorkerNode>> ReceivedWorkerNodesData;

		public CheckTablesInManagerDbTimer(string connectionString,double interval = 5000)
		{
			ManagerDbRepository = new ManagerDbRepository(connectionString);

			JobQueueTimer = new Timer(interval);
			JobQueueTimer.Elapsed += JobQueueTimer_Elapsed;

			JobTimer = new Timer(interval);
			JobTimer.Elapsed += JobTimer_Elapsed;

			JobDetailTimer = new Timer(interval);
			JobDetailTimer.Elapsed += JobDetailTimer_Elapsed;

			LoggingTimer = new Timer(interval);
			LoggingTimer.Elapsed += LoggingTimerOnElapsed;

			WorkerNodeTimer = new Timer(interval);
			WorkerNodeTimer.Elapsed += WorkerNodeTimerOnElapsed;
		}

		public Timer WorkerNodeTimer { get; set; }

		public Timer LoggingTimer { get; set; }

		public Timer JobDetailTimer { get; set; }

		public Timer JobTimer { get; set; }

		public ManagerDbRepository ManagerDbRepository { get; set; }

		public Timer JobQueueTimer { get; private set; }

		private void WorkerNodeTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (ReceivedWorkerNodesData != null)
			{
				ReceivedWorkerNodesData(this, ManagerDbRepository.WorkerNodes);
			}
		}

		private void LoggingTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (ReceivedLoggingData != null)
			{
				ReceivedLoggingData(this, ManagerDbRepository.Loggings);
			}
		}

		private void JobDetailTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobDetailItem != null)
			{
				ReceivedJobDetailItem(this, ManagerDbRepository.JobDetails);
			}
		}

		private void JobTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobItem != null)
			{
				ReceivedJobItem(this, ManagerDbRepository.Jobs);
			}
		}

		private void JobQueueTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobQueueItem != null)
			{
				ReceivedJobQueueItem(this, ManagerDbRepository.JobQueueItems);
			}
		}

		public void Dispose()
		{
			JobQueueTimer.Stop();
			JobQueueTimer.Dispose();

			JobTimer.Stop();
			JobTimer.Dispose();

			JobDetailTimer.Stop();
			JobDetailTimer.Dispose();

			LoggingTimer.Stop();
			LoggingTimer.Dispose();

			WorkerNodeTimer.Stop();
			WorkerNodeTimer.Dispose();
		}
	}
}