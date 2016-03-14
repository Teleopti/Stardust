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
			if (httpSender == null)
			{
				throw new ArgumentNullException("httpSender");
			}

			nodeConfiguration.ThrowArgumentNullException();

			HttpSender = httpSender;
			NodeConfiguration = nodeConfiguration;

			CancellationTokenSource = new CancellationTokenSource();

			JobProgressModels =
				new ConcurrentDictionary<DateTime, ISendJobProgressModel>();

			WhoamI =
				NodeConfiguration.CreateWhoIAm(Environment.MachineName);

			UriBuilder =
				new UriBuilder(NodeConfiguration.ManagerLocation);

			UriBuilder.Path += ManagerRouteConstants.JobProgress;

			Elapsed += OnTimedEvent;
		}

		private UriBuilder UriBuilder { get; set; }

		public string WhoamI { get; private set; }

		public Guid JobId { get; set; }

		private INodeConfiguration NodeConfiguration { get; set; }

		private IHttpSender HttpSender { get; set; }

		private ConcurrentDictionary<DateTime, ISendJobProgressModel> JobProgressModels { get; set; }

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void ClearAllJobProgresses(Guid jobId)
		{
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
		}

		public void ClearAllJobProgresses()
		{
			if (JobProgressModels != null)
			{
				JobProgressModels.Clear();
			}
		}

		public bool HasAllProgressesBeenSent(Guid jobId)
		{
			if (JobProgressModels == null)
			{
				return true;
			}

			var allSent = JobProgressModels.Values.All(model => model.ResponseMessage != null &&
																model.ResponseMessage.IsSuccessStatusCode &&
																model.JobId == jobId);

			return allSent;
		}

		public bool HasAllProgressesBeenSent()
		{
			if (JobProgressModels == null)
			{
				return true;
			}

			var allSent = JobProgressModels.Values.All(model => model.ResponseMessage != null &&
			                                                    model.ResponseMessage.IsSuccessStatusCode);

			return allSent;
		}

		protected virtual async void TrySendJobProgress()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start sending job progress to manager.");

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
					foreach (var sendJobProgressModel in sendJobProgressModels)
					{
						sendJobProgressModel.ResponseMessage =
							await HttpSender.PostAsync(UriBuilder.Uri,
							                           new
							                           {
								                           sendJobProgressModel.JobId,
								                           sendJobProgressModel.ProgressDetail
							                           });
					}
				}
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Finished send job progress to manager.");
		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			TrySendJobProgress();
		}

		public void SendProgress(Guid jobid,
		                         string progressMessage)
		{
			var progressModel = new SendJobProgressModel
			{
				JobId = jobid,
				ProgressDetail = progressMessage
			};

			JobProgressModels.AddOrUpdate(DateTime.Now,
			                              progressModel,
			                              (time, model) => progressModel);
		}

		protected override void Dispose(bool disposing)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			base.Dispose(disposing);

			LogHelper.LogDebugWithLineNumber(Logger, "Finished disposing.");
		}
	}
}