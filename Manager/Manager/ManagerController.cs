using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Stardust.Manager.ActionResults;
using Stardust.Manager.Constants;
using Stardust.Manager.Extensions;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerController : ApiController
	{
		private readonly JobManager _jobManager;

		private readonly INodeManager _nodeManager;

		public ManagerController(INodeManager nodeManager,
		                         JobManager jobManager)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;
		}

		const string JobToDoIsNull = "Job to do can not be null.";

		const string NodeUriIsInvalid = "Node Uri is invalid.";

		const string JobIdIsInvalid = "Job Id is invalid.";

		const string JobToDoNameIsInvalid = "Job to do property=NAME is invalid.";

		const string JobToDoUserNameIsInvalid = "Job to do property=USERNAME is invalid.";

		const string JobToDoSerializedIsInvalid = "Job to do property=SERIALIZED is invalid.";

		const string JobToDoTypeIsNullOrEmpty = "Job to do property=TYPE can not be null or empty string.";



		[HttpPost, Route(ManagerRouteConstants.Job)]
		public IHttpActionResult DoThisJob([FromBody] JobRequestModel job)
		{
			if (job == null)
			{
				return new BadRequestWithReasonPhrase(JobToDoIsNull);
			}

			if (string.IsNullOrEmpty(job.Name))
			{
				return new BadRequestWithReasonPhrase(JobToDoNameIsInvalid);
			}

			if (string.IsNullOrEmpty(job.Type))
			{
				return new BadRequestWithReasonPhrase(JobToDoTypeIsNullOrEmpty);
			}

			if (string.IsNullOrEmpty(job.UserName))
			{
				return new BadRequestWithReasonPhrase(JobToDoUserNameIsInvalid);
			}

			if (string.IsNullOrEmpty(job.Serialized))
			{
				return new BadRequestWithReasonPhrase(JobToDoSerializedIsInvalid);
			}

			var jobReceived = new JobDefinition
			{
				Name = job.Name,
				Serialized = job.Serialized,
				Type = job.Type,
				UserName = job.UserName,
				Id = Guid.NewGuid()
			};
			_jobManager.Add(jobReceived);

			var msg = string.Format("{0} : New job received from client ( jobId, jobName ) : ( {1}, {2} )",
			                        WhoAmI(Request),
			                        jobReceived.Id,
			                        jobReceived.Name);

			this.Log().InfoWithLineNumber(msg);

			Task.Factory.StartNew(() =>
			{
				_jobManager.CheckAndAssignNextJob();
			});

			return Ok(jobReceived.Id);
		}

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelThisJob(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase(JobIdIsInvalid);
			}

			this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job cancel from client ( jobId ) : ( " + jobId + " )");

			_jobManager.CancelThisJob(jobId);

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistoryList)]
		public IHttpActionResult JobHistoryList()
		{
			var jobHistory = _jobManager.GetJobHistoryList();

			return Ok(jobHistory);
		}

		[HttpGet, Route(ManagerRouteConstants.GetJobHistory)]
		public IHttpActionResult JobHistory(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase(JobIdIsInvalid);
			}

			var jobHistory = _jobManager.GetJobHistory(jobId);

			return Ok(jobHistory);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase(JobIdIsInvalid);
			}

			var jobHistoryDetail = _jobManager.JobHistoryDetails(jobId);

			return Ok(jobHistoryDetail);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult Heartbeat([FromBody] Uri nodeUri)
		{
			if (nodeUri == null)
			{
				return new BadRequestWithReasonPhrase(NodeUriIsInvalid);
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received heartbeat from Node. Node Uri : ( " + nodeUri + " )");

				_jobManager.RegisterHeartbeat(nodeUri.ToString());
			});			

			return Ok();	
		}

		[HttpPost, Route(ManagerRouteConstants.JobDone)]
		public IHttpActionResult JobDone(Guid jobId)
		{			
			if (jobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase(JobIdIsInvalid);
			}
			
			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received job done from a Node ( jobId ) : ( " + jobId + " )");

				_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
				                                         "Success");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobFailed)]
		public IHttpActionResult JobFailed([FromBody] JobFailedModel jobFailedModel)
		{
			if (jobFailedModel == null)
			{
				return new BadRequestWithReasonPhrase("Invalid job failed model.");
			}

			if (jobFailedModel.JobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase("Invalid job id on job failed model.");
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().ErrorWithLineNumber(WhoAmI(Request) + ": Received job failed from a Node ( jobId ) : ( " +
											   jobFailedModel.JobId + " )");

				var progress = new JobProgressModel
				{
					JobId = jobFailedModel.JobId,
					Created = DateTime.Now,
					ProgressDetail = jobFailedModel.AggregateException.ToString()
				};

				_jobManager.ReportProgress(progress);

				_jobManager.SetEndResultOnJobAndRemoveIt(jobFailedModel.JobId,
				                                         "Failed");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobHasBeenCanceled)]
		public IHttpActionResult JobCanceled(Guid jobId)
		{
			if (jobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase(JobIdIsInvalid);
			}			

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received cancel from a Node ( jobId ) : ( " + jobId + " )");

				_jobManager.SetEndResultOnJobAndRemoveIt(jobId,
				                                         "Canceled");

				_jobManager.CheckAndAssignNextJob();
			});

			return Ok();
		}

		[HttpPost, Route(ManagerRouteConstants.JobProgress)]
		public IHttpActionResult JobProgress([FromBody] JobProgressModel model)
		{
			if (model == null)
			{
				return new BadRequestWithReasonPhrase("Job progress model can not be null.");
			}

			if (model.JobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase("Job progress model ID can not be null.");
			}

			if (string.IsNullOrEmpty(model.ProgressDetail))
			{
				return new BadRequestWithReasonPhrase("Job progress model PROGRESSDETAIL can not be null.");
			}

			Task.Factory.StartNew(() =>
			{
				_jobManager.ReportProgress(model);
			});

			return Ok();
		}

		// to handle that scenario where the node comes up after a crash
		//this end point should be called when the node comes up
		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri nodeUri)
		{
			if (nodeUri == null)
			{
				return new BadRequestWithReasonPhrase("Node Uri is invalid");
			}

			Task.Factory.StartNew(() =>
			{
				this.Log().InfoWithLineNumber(WhoAmI(Request) + ": Received init from Node. Node Uri : ( " + nodeUri + " )");

				_nodeManager.FreeJobIfAssingedToNode(nodeUri);
				_nodeManager.AddIfNeeded(nodeUri);
				_jobManager.CheckAndAssignNextJob();
			});			

			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Ping)]
		public IHttpActionResult Ping()
		{
			return Ok();
		}

		[HttpGet, Route(ManagerRouteConstants.Nodes)]
		public IHttpActionResult Nodes()
		{
			var workernodes = _jobManager.Nodes();

			return Ok(workernodes);
		}

		private string WhoAmI(HttpRequestMessage request)
		{
			if (request == null)
			{
				return "[MANAGER, " + Environment.MachineName.ToUpper() + "]";
			}

			var baseUrl =
				request.RequestUri.GetLeftPart(UriPartial.Authority);

			return "[MANAGER, " + baseUrl + ", " + Environment.MachineName.ToUpper() + "]";

		}
	}
}