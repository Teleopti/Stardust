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


	public class ThreadPool : IThreadPool, IDisposable
	{
		private DoWork _doWork;
		private readonly object _lockObject = new object();
		private Dictionary<BackgroundWorker, ManualResetEventSlim> _bgw;
		private Queue<ITaskParameters> _taskQueue;

		public event RowsUpdatedEventHandler RowsUpdatedEvent;

		public Exception ThreadError => _threadError;

		public void Load(IEnumerable<ITaskParameters> taskParameters, DoWork doWork)
		{
			if (!int.TryParse(ConfigurationManager.AppSettings["concurrentThreads"], out var aff))
			{
				aff = 1;
			}

			_bgw = new Dictionary<BackgroundWorker,ManualResetEventSlim>(aff);

			for (int i = 0; i < aff; i++)
			{
				var worker = new BackgroundWorker();
				worker.DoWork += worker_DoWork;
				worker.RunWorkerCompleted += workerRunWorkerCompleted;

				_bgw.Add(worker, new ManualResetEventSlim());
			}

			_doWork = doWork;
			_taskQueue = new Queue<ITaskParameters>(taskParameters);
		}

		public void Start()
		{
			lock (_lockObject)
			{
				foreach (var worker in _bgw)
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
							worker.Key.RunWorkerAsync(task);
						}
					}
				}
			}

			foreach (var manualResetEventSlim in _bgw)
			{
				manualResetEventSlim.Value.Wait();
			}
		}
		
		private Exception _threadError;

		void workerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var backgroundWorker = (BackgroundWorker)sender;
			if (ThreadError != null)
			{
				_bgw[backgroundWorker].Set();
				return;
			}

			if (e.Error != null)
			{
				_threadError = new ScheduleTransformerException(e.Error.Message, e.Error.StackTrace);
				_bgw[backgroundWorker].Set();
				return;
			}
			var rowsUpdated = (int)e.Result;

			RowsUpdatedEvent?.Invoke(this, new RowsUpdatedEventArgs(rowsUpdated));

			lock (_lockObject)
			{

				if (_taskQueue.Count > 0)
				{
					var task = _taskQueue.Dequeue();
					backgroundWorker.RunWorkerAsync(task);
				}
				else
				{
					_bgw[backgroundWorker].Set();
				}
			}
		}

		void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			var taskParameters = (ITaskParameters)e.Argument;

			Thread.CurrentThread.CurrentCulture = taskParameters.JobParameters.CurrentCulture;

			e.Result = _doWork(taskParameters);
		}

		public void Dispose()
		{
			if (_bgw == null)
				return;
			foreach (var item in _bgw)
			{
				item.Value?.Dispose();
				item.Key?.Dispose();
			}
			_bgw.Clear();
		}
	}
}