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
using Stardust.Node.Workers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendJobDetailToManagerTimer : Timer
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (TrySendJobDetailToManagerTimer));

		
		public EventHandler<IJobDetail> SendJobDetailWithSuccessEventHandler;

		private void InvokeSendJobProgressModelWithSuccessEventHandler(IJobDetail jobDetail)
		{
			if (SendJobDetailWithSuccessEventHandler != null)
			{
				SendJobDetailWithSuccessEventHandler(this, jobDetail);
			}
		}

		public TrySendJobDetailToManagerTimer(NodeConfiguration nodeConfiguration,
		                                        IHttpSender httpSender,
		                                        double interval = 500) : base(interval)
		{
			_httpSender = httpSender;
			_cancellationTokenSource = new CancellationTokenSource();
			_jobDetails = new ConcurrentDictionary<Guid, IJobDetail>();
			_uriBuilder = new UriBuilder(nodeConfiguration.ManagerLocation);
			_uriBuilder.Path += ManagerRouteConstants.JobProgress;

			Elapsed += OnTimedEvent;
		}
		
		private readonly UriBuilder _uriBuilder;
		private readonly IHttpSender _httpSender;
		private readonly ConcurrentDictionary<Guid, IJobDetail> _jobDetails;
		private readonly CancellationTokenSource _cancellationTokenSource;


		public void ClearAllJobProgresses(Guid jobId)
		{
			if (_jobDetails != null)
			{
				var progressesToRemove =
					_jobDetails.Where(pair => pair.Value.JobId == jobId);

				foreach (var progress in progressesToRemove)
				{
					IJobDetail model;
					_jobDetails.TryRemove(progress.Key, out model);
				}
			}
		}


		public int TotalNumberOfJobProgresses(Guid jobId)
		{
			if (IsJobProgressesNullOrEmpty())
			{
				return 0;
			}
			return _jobDetails.Count(pair => pair.Value.JobId == jobId);
		}


		private bool IsJobProgressesNullOrEmpty()
		{
			return _jobDetails == null || _jobDetails.Count == 0;
		}


		public bool HasAllProgressesBeenSent(Guid jobId)
		{
			if (_jobDetails == null)
			{
				return true;
			}

			var allSent =
				_jobDetails.Values.Count(model => model.JobId == jobId) == 0;

			return allSent;
		}

		public bool HasAllProgressesBeenSent()
		{

			if (_jobDetails == null)
			{
				return true;
			}

			var allSent = _jobDetails.Values.Count == 0;
			
			return allSent;
		}

		protected virtual void TrySendJobProgress()
		{
			if (_jobDetails != null && _jobDetails.Count > 0)
			{
				foreach (var sendJobProgressModel in _jobDetails)
				{
					try
					{
						var model = sendJobProgressModel;

						var task = Task.Factory.StartNew(async () =>
						{
							JobDetailEntity jobDetail = new JobDetailEntity
							{
								JobId = model.Value.JobId,
								Detail = model.Value.Detail,
								Created = model.Value.Created
							};

							var httpResponseMessage =
								await _httpSender.PostAsync(_uriBuilder.Uri,
														   jobDetail);

							if (httpResponseMessage.IsSuccessStatusCode)
							{
								IJobDetail removedValue;

								var removed =
									_jobDetails.TryRemove(model.Key, out removedValue);

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
		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			Stop();

			try
			{
				TrySendJobProgress();
			}

			finally
			{
				Start();
			}
		}

		public void SendProgress(Guid jobid,
		                         string progressMessage)
		{

			var progressModel = new JobDetailEntity
			{
				JobId = jobid,
				Detail = progressMessage,
				Created = DateTime.UtcNow
			};

			_jobDetails.TryAdd(Guid.NewGuid(), progressModel);
		}

		protected override void Dispose(bool disposing)
		{
			if (_cancellationTokenSource != null &&
			    !_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
			base.Dispose(disposing);
		}

		public int TotalNumberOfJobProgresses()
		{
			return _jobDetails.Count;
		}
	}
}