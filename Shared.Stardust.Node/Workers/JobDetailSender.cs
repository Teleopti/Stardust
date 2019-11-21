using System;
using System.Collections.Concurrent;
using System.Threading;
using log4net;
using Stardust.Node.Constants;
using Stardust.Node.Entities;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	public class JobDetailSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(JobDetailSender));
		private UriBuilder _uriBuilder;
		private readonly IHttpSender _httpSender;
		private readonly ConcurrentStack<JobDetailEntity> _jobDetails;

		public JobDetailSender(IHttpSender httpSender)
		{
			_httpSender = httpSender;
			_jobDetails = new ConcurrentStack<JobDetailEntity>();
		}
        
		public int DetailsCount()
		{
			return _jobDetails.Count;
		}

        public void AddDetail(Guid jobId, string progressMessage)
        {
            _jobDetails.Push(new JobDetailEntity
            {
                JobId = jobId,
                Detail = progressMessage,
                Created = DateTime.UtcNow,
                Sent = false
            });
        }

        public void Send(CancellationToken cancellationToken)
		{
			if (_jobDetails == null || _jobDetails.Count <= 0 || _uriBuilder == null) return;

            var count = _jobDetails.Count;
            var items = new JobDetailEntity[count];
            if (_jobDetails.TryPopRange(items) > 0)
            {
                try
                {
                    var httpResponseMessage = _httpSender.PostAsync(_uriBuilder.Uri, items, cancellationToken)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                    httpResponseMessage.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    Logger.ErrorWithLineNumber(
                        $"Send job progresses to manager failed for job ( jobId ) : ( {items[0]?.JobId} )");
                }
            }
        }

		public void SetManagerLocation(Uri managerLocation)
		{
			_uriBuilder = new UriBuilder(managerLocation);
			_uriBuilder.Path += ManagerRouteConstants.JobProgress;
		}
	}
}
