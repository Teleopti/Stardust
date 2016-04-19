using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Manager.Integration.Test.Data;

namespace Manager.Integration.Test.Timers
{
	public class CheckTablesInManagerDbTimer
	{
		public EventHandler<List<Manager.Integration.Test.Data.JobQueue>> ReceivedJobQueueItem;

		public EventHandler<List<Manager.Integration.Test.Data.Job>> ReceivedJobItem;

		public EventHandler<List<Manager.Integration.Test.Data.JobDetail>> ReceivedJobDetailItem;

		public EventHandler<List<Manager.Integration.Test.Data.Logging>> ReceivedLoggingData;

		public EventHandler<List<Manager.Integration.Test.Data.WorkerNode>> ReceivedWorkerNodesData;

		public CheckTablesInManagerDbTimer(double interval = 5000)
		{
			ManagerDbEntities = new ManagerDbEntities();

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

		private ManagerDbEntities ManagerDbEntities { get; set; }

		public Timer JobQueueTimer { get; private set; }

		private void WorkerNodeTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (ReceivedWorkerNodesData != null)
			{
				ReceivedWorkerNodesData(this, ManagerDbEntities.WorkerNodes.ToList());
			}
		}

		private void LoggingTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (ReceivedLoggingData != null)
			{
				ReceivedLoggingData(this, ManagerDbEntities.Loggings.ToList());
			}
		}

		private void JobDetailTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobDetailItem != null)
			{
				ReceivedJobDetailItem(this, ManagerDbEntities.JobDetails.ToList());
			}
		}

		private void JobTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobItem != null)
			{
				ReceivedJobItem(this, ManagerDbEntities.Jobs.ToList());
			}
		}

		private void JobQueueTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobQueueItem != null)
			{
				ReceivedJobQueueItem(this, ManagerDbEntities.JobQueues.ToList());
			}
		}
	}
}