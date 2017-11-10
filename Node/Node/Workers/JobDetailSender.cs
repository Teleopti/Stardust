﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
		private readonly UriBuilder _uriBuilder;
		private readonly IHttpSender _httpSender;
		private readonly List<JobDetailEntity> _jobDetails;

		public JobDetailSender(NodeConfiguration nodeConfiguration,
												IHttpSender httpSender)
		{
			_httpSender = httpSender;
			_jobDetails = new List<JobDetailEntity>();
			_uriBuilder = new UriBuilder(nodeConfiguration.ManagerLocation);
			_uriBuilder.Path += ManagerRouteConstants.JobProgress;
		}


		public int DetailsCount()
		{
			return _jobDetails.Count;
		}


		public void AddDetail(Guid jobid, string progressMessage)
		{
			lock (_jobDetails)
			{
				_jobDetails.Add(new JobDetailEntity
				{
					JobId = jobid,
					Detail = progressMessage,
					Created = DateTime.UtcNow,
					Sent = false
				});
			}
		}


		public void Send(CancellationToken cancellationToken)
		{
			if (_jobDetails == null || _jobDetails.Count <= 0) return;
			lock (_jobDetails)
			{
				try
				{
					var task = Task.Factory.StartNew(async () =>
					                                 {
						                                 var httpResponseMessage = await _httpSender.PostAsync(_uriBuilder.Uri, _jobDetails, cancellationToken);
						                                 if (httpResponseMessage.IsSuccessStatusCode)
						                                 {
							                                 //detail.Sent = true;
						                                 }
					                                 }, cancellationToken);
					task.Wait(cancellationToken);
					_jobDetails.Clear();
				}
				catch (Exception)
				{
					Logger.ErrorWithLineNumber($"Send job progresses to manager failed for job ( jobId ) : ( {_jobDetails.FirstOrDefault().JobId} )");
				}
			}
		}
	}
}
