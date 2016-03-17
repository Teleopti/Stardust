using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

namespace Stardust.Node.Timers
{
	public class TrySendJobFaultedToManagerTimer : TrySendStatusToManagerTimer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TrySendJobFaultedToManagerTimer));

		public TrySendJobFaultedToManagerTimer(INodeConfiguration nodeConfiguration,
		                                       TrySendJobProgressToManagerTimer sendJobProgressToManagerTimer,
		                                       double interval = 2000) : base(nodeConfiguration,
		                                                                      nodeConfiguration
			                                                                      .GetManagerJobHasFaileTemplatedUri(),
		                                                                      sendJobProgressToManagerTimer,
		                                                                      interval)
		{
		}


		public AggregateException AggregateExceptionToSend { get; set; }

		public DateTime? ErrorOccured { get; set; }

		protected override Task<HttpResponseMessage> TrySendStatus(JobToDo jobToDo,
		                                                           CancellationToken cancellationToken)
		{
			try
			{
				var httpSender = new HttpSender();

				var payload = new JobFailedModel
				{
					JobId = jobToDo.Id,
					AggregateException = AggregateExceptionToSend,
					Created = ErrorOccured
				};

				var response =
					httpSender.PostAsync(CallbackTemplateUri,
					                     payload,
					                     cancellationToken);
				return response;
			}

			catch (Exception exp)
			{
				Logger.ErrorWithLineNumber("Error in TrySendStatus.",
				                                 exp);
				throw;
			}
		}

		protected override void Dispose(bool disposing)
		{
			Logger.DebugWithLineNumber("Start disposing.");

			base.Dispose(disposing);

			Logger.DebugWithLineNumber("Finished disposing.");
		}
	}
}