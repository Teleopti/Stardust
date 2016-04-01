using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendJobProgressToManagerTimer : Timer
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (TrySendJobProgressToManagerTimer));

		public EventHandler<ISendJobProgressModel> SendJobProgressModelWithSuccessEventHandler;


		private void InvokeSendJobProgressModelWithSuccessEventHandler(ISendJobProgressModel jobProgressModel)
		{
			if (SendJobProgressModelWithSuccessEventHandler != null)
			{
				SendJobProgressModelWithSuccessEventHandler(this, jobProgressModel);
			}
		}

		public TrySendJobProgressToManagerTimer(INodeConfiguration nodeConfiguration,
		                                        IHttpSender httpSender,
		                                        double interval) : base(interval)
		{
			Logger.DebugWithLineNumber("Start.");

			if (httpSender == null)
			{
				throw new ArgumentNullException("httpSender");
			}

			nodeConfiguration.ThrowArgumentNullException();

			HttpSender = httpSender;
			NodeConfiguration = nodeConfiguration;

			CancellationTokenSource = new CancellationTokenSource();

			JobProgressModels =
				new ConcurrentDictionary<Guid, ISendJobProgressModel>();

			WhoamI =
				NodeConfiguration.CreateWhoIAm(Environment.MachineName);

			UriBuilder =
				new UriBuilder(NodeConfiguration.ManagerLocation);

			UriBuilder.Path += ManagerRouteConstants.JobProgress;

			Elapsed += OnTimedEvent;

			Logger.DebugWithLineNumber("Finished.");
		}


		private UriBuilder UriBuilder { get; set; }

		public string WhoamI { get; private set; }

		public Guid JobId { get; set; }

		private INodeConfiguration NodeConfiguration { get; set; }

		private IHttpSender HttpSender { get; set; }

		private ConcurrentDictionary<Guid, ISendJobProgressModel> JobProgressModels { get; set; }

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void ClearAllJobProgresses(Guid jobId)
		{
			Logger.DebugWithLineNumber("Start.");

			if (jobId == Guid.Empty)
			{
				throw new ArgumentNullException("jobId");
			}

			if (JobProgressModels != null)
			{
				var progressesToRemove =
					JobProgressModels.Where(pair => pair.Value.JobId == jobId);

				foreach (var progress in progressesToRemove)
				{
					ISendJobProgressModel model;

					var tryRemove =
						JobProgressModels.TryRemove(progress.Key,
						                            out model);
				}
			}

			Logger.DebugWithLineNumber("Finished.");
		}

		public int TotalNumberOfJobProgresses(Guid jobId)
		{
			if (IsJobProgressesNullOrEmpty())
			{
				return 0;
			}

			return JobProgressModels.Count(pair => pair.Value.JobId == jobId);
		}

		private bool IsJobProgressesNullOrEmpty()
		{
			return JobProgressModels == null || JobProgressModels.Count == 0;
		}

		public void ClearAllJobProgresses()
		{
			Logger.DebugWithLineNumber("Start.");

			if (JobProgressModels != null)
			{
				JobProgressModels.Clear();
			}

			Logger.DebugWithLineNumber("Finished.");
		}

		public bool HasAllProgressesBeenSent(Guid jobId)
		{
			Logger.DebugWithLineNumber("Start.");

			if (JobProgressModels == null)
			{
				return true;
			}

			var allSent =
				JobProgressModels.Values.Count(model => model.JobId == jobId) == 0;

			Logger.DebugWithLineNumber("Finished.");

			return allSent;
		}

		public bool HasAllProgressesBeenSent()
		{
			Logger.DebugWithLineNumber("Start.");

			if (JobProgressModels == null)
			{
				return true;
			}

			var allSent = JobProgressModels.Values.Count == 0;

			Logger.DebugWithLineNumber("Finished.");

			return allSent;
		}

		protected virtual void TrySendJobProgress()
		{
			Logger.DebugWithLineNumber("Start.");

			if (JobProgressModels != null && JobProgressModels.Count > 0)
			{
				foreach (var sendJobProgressModel in JobProgressModels)
				{
					Logger.DebugWithLineNumber("Try send job progress ( jobId, progressDetail ) : ( " +
					                           sendJobProgressModel.Value.JobId +
					                           ", " + sendJobProgressModel.Value.ProgressDetail + " )");

					try
					{
						var model = sendJobProgressModel;

						var task = Task.Factory.StartNew(async () =>
						{
							var httpResponseMessage =
								await HttpSender.PostAsync(UriBuilder.Uri,
								                           new
								                           {
									                           model.Value.JobId,
									                           model.Value.ProgressDetail,
									                           model.Value.Created
								                           });

							if (httpResponseMessage.IsSuccessStatusCode)
							{
								ISendJobProgressModel removedValue;

								var removed =
									JobProgressModels.TryRemove(model.Key, out removedValue);

								if (removed)
								{
									InvokeSendJobProgressModelWithSuccessEventHandler(removedValue);
								}
							}
						});

						task.Wait();
					}
					catch (Exception)
					{
						var msg =
							string.Format("Send job progresses to manager failed for job ( jobId ) : ( {0} )",
							              sendJobProgressModel.Value.JobId);

						Logger.ErrorWithLineNumber(msg);
					}
				}
			}

			Logger.DebugWithLineNumber("Finished.");
		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			Logger.DebugWithLineNumber("Start.");

			Stop();

			try
			{
				TrySendJobProgress();
			}

			finally
			{
				Start();

				Logger.DebugWithLineNumber("Finished.");
			}
		}

		public void SendProgress(Guid jobid,
		                         string progressMessage)
		{
			Logger.DebugWithLineNumber("Start.");

			var progressModel = new SendJobProgressModel
			{
				JobId = jobid,
				ProgressDetail = progressMessage,
				Created = DateTime.UtcNow
			};

			JobProgressModels.TryAdd(Guid.NewGuid(), progressModel);

			Logger.DebugWithLineNumber("Finished.");
		}

		protected override void Dispose(bool disposing)
		{
			Logger.DebugWithLineNumber("Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			base.Dispose(disposing);

			Logger.DebugWithLineNumber("Finished disposing.");
		}

		public int TotalNumberOfJobProgresses()
		{
			return JobProgressModels.Count;
		}
	}
}