using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Manager.Integration.Test.Data;

namespace Manager.Integration.Test.Timers
{
	public class CheckTablesInManagerDbTimer
	{
		public EventHandler<List<JobDefinition>> ReceivedJobDefinitionsData;

		public EventHandler<List<Data.JobHistory>> ReceivedJobHistoriesData;

		public EventHandler<List<JobHistoryDetail>> ReceivedJobHistoryDetailsData;

		public EventHandler<List<Logging>> ReceivedLoggingData;

		public EventHandler<List<WorkerNode>> ReceivedWorkerNodesData;

		public CheckTablesInManagerDbTimer(double interval = 5000)
		{
			ManagerDbEntities = new ManagerDbEntities();

			JobDefinitionsTimer = new Timer(interval);
			JobDefinitionsTimer.Elapsed += JobDefinitionsTimer_Elapsed;

			JobHistoryTimer = new Timer(interval);
			JobHistoryTimer.Elapsed += JobHistoryTimer_Elapsed;

			JobHistoryDetailsTimer = new Timer(interval);
			JobHistoryDetailsTimer.Elapsed += JobHistoryDetailsTimer_Elapsed;

			LoggingsTimer = new Timer(interval);
			LoggingsTimer.Elapsed += LoggingsTimerOnElapsed;

			WorkerNodesTimer = new Timer(interval);
			WorkerNodesTimer.Elapsed += WorkerNodesTimerOnElapsed;
		}

		public Timer WorkerNodesTimer { get; set; }

		public Timer LoggingsTimer { get; set; }

		public Timer JobHistoryDetailsTimer { get; set; }

		public Timer JobHistoryTimer { get; set; }

		private ManagerDbEntities ManagerDbEntities { get; set; }

		public Timer JobDefinitionsTimer { get; private set; }

		private void WorkerNodesTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (ReceivedWorkerNodesData != null)
			{
				ReceivedWorkerNodesData(this, ManagerDbEntities.WorkerNodes.ToList());
			}
		}

		private void LoggingsTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (ReceivedLoggingData != null)
			{
				ReceivedLoggingData(this, ManagerDbEntities.Loggings.ToList());
			}
		}

		private void JobHistoryDetailsTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobHistoryDetailsData != null)
			{
				ReceivedJobHistoryDetailsData(this, ManagerDbEntities.JobHistoryDetails.ToList());
			}
		}

		private void JobHistoryTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobHistoriesData != null)
			{
				ReceivedJobHistoriesData(this, ManagerDbEntities.JobHistories.ToList());
			}
		}

		private void JobDefinitionsTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (ReceivedJobDefinitionsData != null)
			{
				ReceivedJobDefinitionsData(this, ManagerDbEntities.JobDefinitions.ToList());
			}
		}
	}
}