using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public delegate int DoWork(ITaskParameters parameters);

	public class RowsUpdatedEventArgs : EventArgs
	{
		public RowsUpdatedEventArgs(int affectedRows)
		{
			AffectedRows = affectedRows;
		}

		public int AffectedRows { set; get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void RowsUpdatedEventHandler(object sender, RowsUpdatedEventArgs e);


	public class ThreadPool : IThreadPool
	{
		private static DoWork _doWork;
		private readonly object _lockObject = new object();
		private BackgroundWorker[] _bgw;
		private Queue<ITaskParameters> _taskQueue;

		public event RowsUpdatedEventHandler RowsUpdatedEvent;

		public Exception ThreadError
		{
			get { return _threadError; }
		}

		public void Load(IEnumerable<ITaskParameters> taskParameters, DoWork doWork)
		{
			int aff;
			if (!int.TryParse(ConfigurationManager.AppSettings["concurrentThreads"], out aff))
			{
				aff = 1;
			}

			_bgw = new BackgroundWorker[aff];

			for (int i = 0; i < aff; i++)
			{
				_bgw[i] = new BackgroundWorker();
				_bgw[i].DoWork += worker_DoWork;
				_bgw[i].RunWorkerCompleted += workerRunWorkerCompleted;
			}

			_doWork = doWork;
			_taskQueue = new Queue<ITaskParameters>(taskParameters);
		}

		public void Start()
		{
			lock (_lockObject)
			{
				foreach (BackgroundWorker worker in _bgw)
				{
					if (_taskQueue.Count > 0)
					{
						if (ThreadError != null)
						{
							break;
						}
						var task = _taskQueue.Dequeue();
						if (task != null)
						{
							_busyThreads++;
							worker.RunWorkerAsync(task);
						}
					}
				}
			}


			while (_busyThreads > 0)
			{
				Thread.Sleep(500);
			}

		}

		private int _busyThreads;
		private Exception _threadError;

		void workerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (ThreadError != null)
			{
				return;
			}
			if (e.Error != null)
			{
				_threadError = new ScheduleTransformerException(e.Error.Message, e.Error.StackTrace);
				_busyThreads = 0;
				return;
			}
			var rowsUpdated = (int)e.Result;

			if (RowsUpdatedEvent != null)
				RowsUpdatedEvent(this, new RowsUpdatedEventArgs(rowsUpdated));

			lock (_lockObject)
			{

				if (_taskQueue.Count > 0)
				{
					var task = _taskQueue.Dequeue();
					((BackgroundWorker)sender).RunWorkerAsync(task);
				}
				else
				{
					_busyThreads--;
				}
			}
		}

		void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			var taskParameters = (ITaskParameters)e.Argument;

			Thread.CurrentThread.CurrentCulture = taskParameters.JobParameters.CurrentCulture;

			e.Result = _doWork(taskParameters);
		}
	}
}