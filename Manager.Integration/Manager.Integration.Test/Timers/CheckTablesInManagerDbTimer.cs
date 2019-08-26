using System;
using System.Collections.ObjectModel;
using System.Timers;
using Manager.Integration.Test.Database;
using Manager.Integration.Test.Models;

namespace Manager.Integration.Test.Timers
{
	public class CheckTablesInManagerDbTimer : IDisposable
	{
		public EventHandler<ObservableCollection<JobQueueItem>> GetJobQueueItems;

		public EventHandler<ObservableCollection<Job>> GetJobItems;

		public EventHandler<ObservableCollection<JobDetail>> GetJobDetailItems;

		public EventHandler<ObservableCollection<Logging>> GetLogging;

		public EventHandler<ObservableCollection<WorkerNode>> GetWorkerNodes;

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
            GetWorkerNodes?.Invoke(this, ManagerDbRepository.WorkerNodes);
        }

		private void LoggingTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            GetLogging?.Invoke(this, ManagerDbRepository.Loggings);
        }

		private void JobDetailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetJobDetailItems?.Invoke(this, ManagerDbRepository.JobDetails);
        }

		private void JobTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetJobItems?.Invoke(this, ManagerDbRepository.Jobs);
        }

		private void JobQueueTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GetJobQueueItems?.Invoke(this, ManagerDbRepository.JobQueueItems);
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