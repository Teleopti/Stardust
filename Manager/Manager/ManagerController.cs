using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
		private const string NodeUriIsInvalid = "Node Uri is invalid.";

		private const string JobIdIsInvalid = "Job Id is invalid.";

		private readonly JobManager _jobManager;

		private readonly INodeManager _nodeManager;

		public ManagerController(INodeManager nodeManager,
		                         JobManager jobManager)
		{
			_nodeManager = nodeManager;
			_jobManager = jobManager;
		}

		private readonly object _lockCreateNewGuid=new object();

		[HttpPost, Route(ManagerRouteConstants.Job)]
		public IHttpActionResult DoThisJob([FromBody] JobRequestModel job)
		{
			// Validate.
			var isValidRequest = 
				new Validations.Validator().ValidateObject(job, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}
			
			// Start.
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

			Task.Factory.StartNew(() => { _jobManager.CheckAndAssignNextJob(); });

			return Ok(jobReceived.Id);
		}

		private IHttpActionResult ValidateJobId(Guid jobId, HttpRequestMessage requestMessage)
		{
			if (jobId == Guid.Empty)
			{
				return new BadRequestWithReasonPhrase(JobIdIsInvalid);
			}

			if (requestMessage == null)
			{
				requestMessage = new HttpRequestMessage();
			}

			return new OkResult(requestMessage);
		}

		[HttpDelete, Route(ManagerRouteConstants.CancelJob)]
		public IHttpActionResult CancelThisJob(Guid jobId)
		{
			// Validate.
			var isValidRequest = ValidateJobId(jobId, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
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
			// Validate.
			var isValidRequest = ValidateJobId(jobId, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
			var jobHistory = _jobManager.GetJobHistory(jobId);

			return Ok(jobHistory);
		}

		[HttpGet, Route(ManagerRouteConstants.JobDetail)]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			// Validate.
			var isValidRequest = ValidateJobId(jobId, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
			var jobHistoryDetail = _jobManager.JobHistoryDetails(jobId);

			return Ok(jobHistoryDetail);
		}

		private IHttpActionResult ValidateUri(Uri uri,
		                                      HttpRequestMessage requestMessage)
		{
			if (uri == null)
			{
				return new BadRequestWithReasonPhrase(NodeUriIsInvalid);
			}

			if (requestMessage == null)
			{
				requestMessage = new HttpRequestMessage();
			}

			return new OkResult(requestMessage);
		}

		[HttpPost, Route(ManagerRouteConstants.Heartbeat)]
		public IHttpActionResult Heartbeat([FromBody] Uri nodeUri)
		{
			// Validate.
			var isValidRequest = ValidateUri(nodeUri, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
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
			// Validate.
			var isValidRequest = ValidateJobId(jobId, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
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
			// Validate.
			var isValidRequest = 
				new Validations.Validator().ValidateObject(jobFailedModel, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
			Task.Factory.StartNew(() =>
			{
				this.Log().ErrorWithLineNumber(WhoAmI(Request) + ": Received job failed from a Node ( jobId ) : ( " +
				                               jobFailedModel.JobId + " )");

				var progress = new JobProgressModel
				{
					JobId = jobFailedModel.JobId,
					Created = DateTime.UtcNow,
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
			// Validate.
			var isValidRequest = ValidateJobId(jobId, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
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
			// Validate.
			var isValidRequest = 
				new Validations.Validator().ValidateObject(model, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
			Task.Factory.StartNew(() => { _jobManager.ReportProgress(model); });

			return Ok();
		}


		// to handle that scenario where the node comes up after a crash
		//this end point should be called when the node comes up
		[HttpPost, Route(ManagerRouteConstants.NodeHasBeenInitialized)]
		public IHttpActionResult NodeInitialized([FromBody] Uri nodeUri)
		{
			// Validate.
			var isValidRequest = ValidateUri(nodeUri, Request);

			if (!(isValidRequest is OkResult))
			{
				return isValidRequest;
			}

			// Start.
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