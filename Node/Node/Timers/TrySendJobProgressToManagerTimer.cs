using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendJobProgressToManagerTimer : Timer
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (TrySendJobProgressToManagerTimer));


		public TrySendJobProgressToManagerTimer(INodeConfiguration nodeConfiguration,
		                                        IHttpSender httpSender,
		                                        double interval) : base(interval)
		{
			Logger.LogDebugWithLineNumber("Start.");

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

			Logger.LogDebugWithLineNumber("Finished.");
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
			Logger.LogDebugWithLineNumber("Start.");

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

			Logger.LogDebugWithLineNumber("Finished.");

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
			Logger.LogDebugWithLineNumber("Start.");

			if (JobProgressModels != null)
			{
				JobProgressModels.Clear();
			}

			Logger.LogDebugWithLineNumber("Finished.");

		}

		public bool HasAllProgressesBeenSent(Guid jobId)
		{
			Logger.LogDebugWithLineNumber("Start.");

			if (JobProgressModels == null)
			{
				return true;
			}

			var allSent = JobProgressModels.Values.All(model => model.ResponseMessage != null &&
																model.ResponseMessage.IsSuccessStatusCode &&
																model.JobId == jobId);

			Logger.LogDebugWithLineNumber("Finished.");

			return allSent;
		}

		public bool HasAllProgressesBeenSent()
		{
			Logger.LogDebugWithLineNumber("Start.");

			if (JobProgressModels == null)
			{
				return true;
			}

			var allSent = JobProgressModels.Values.All(model => model.ResponseMessage != null &&
			                                                    model.ResponseMessage.IsSuccessStatusCode);

			Logger.LogDebugWithLineNumber("Finished.");

			return allSent;
		}

		protected virtual async void TrySendJobProgress()
		{
			Logger.LogDebugWithLineNumber("Start.");

			if (JobProgressModels == null || JobProgressModels.IsEmpty)
			{
				Logger.LogDebugWithLineNumber("No job progresses to send (outer).");

			}

			if (JobProgressModels != null)
			{
				var notSentProgresses =
					JobProgressModels.Values.Where(model => model.ResponseMessage == null ||
					                                        (model.ResponseMessage != null &&
					                                         !model.ResponseMessage.IsSuccessStatusCode));

				var sendJobProgressModels =
					notSentProgresses as IList<ISendJobProgressModel> ?? notSentProgresses.ToList();

				if (sendJobProgressModels.Any())
				{
					Logger.LogDebugWithLineNumber(sendJobProgressModels.Count +  " job progresses to send.");

					foreach (var sendJobProgressModel in sendJobProgressModels)
					{
						Logger.LogDebugWithLineNumber("Try send job progress ( jobId, progressDetail ) : ( " + sendJobProgressModel.JobId +  ", " + sendJobProgressModel.ProgressDetail + " )");

						try
						{
							sendJobProgressModel.ResponseMessage =
									await HttpSender.PostAsync(UriBuilder.Uri,
												   new
												   {
													   sendJobProgressModel.JobId,
													   sendJobProgressModel.ProgressDetail,
													   sendJobProgressModel.Created
												   });

						}
						catch (Exception)
						{
							var msg =
								string.Format("Send job progresses to manager failed for job ( jobId ) : ( {0} )",
								              sendJobProgressModel.JobId);

							Logger.LogErrorWithLineNumber(msg);
						}
					}
				}
				else
				{
					Logger.LogDebugWithLineNumber("No job progresses to send (inner).");
				}
			}

			Logger.LogDebugWithLineNumber("Finished.");
		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			Logger.LogDebugWithLineNumber("Start.");

			Stop();

			try
			{
				TrySendJobProgress();
			}

			finally
			{
				Start();

				Logger.LogDebugWithLineNumber("Finished.");

			}
		}

		public void SendProgress(Guid jobid,
		                         string progressMessage)
		{
			Logger.LogDebugWithLineNumber("Start.");

			var progressModel = new SendJobProgressModel
			{
				JobId = jobid,
				ProgressDetail = progressMessage,
				Created = DateTime.Now
			};

			JobProgressModels.AddOrUpdate(Guid.NewGuid(), 
			                              progressModel,
			                              (time, model) => progressModel);

			Logger.LogDebugWithLineNumber("Finished.");

		}

		protected override void Dispose(bool disposing)
		{
			Logger.LogDebugWithLineNumber("Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			base.Dispose(disposing);

			Logger.LogDebugWithLineNumber("Finished disposing.");
		}

		public int TotalNumberOfJobProgresses()
		{
			return JobProgressModels.Count;
		}
	}
}