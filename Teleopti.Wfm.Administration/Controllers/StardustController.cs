using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Stardust;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class StardustController : ApiController
	{
		private readonly IStardustRepository _stardustRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IStaffingSettingsReader _staffingSettingsReader;


		public StardustController(IStardustRepository stardustRepository, IEventPublisher eventPublisher,
			ILoadAllTenants loadAllTenants, IStaffingSettingsReader staffingSettingsReader)
		{
			_stardustRepository = stardustRepository;
			_eventPublisher = eventPublisher;
			_loadAllTenants = loadAllTenants;
			_staffingSettingsReader = staffingSettingsReader;
		}

		[HttpGet, Route("Stardust/Jobs/{from}/{to}")]
		public IHttpActionResult JobHistoryList(int from, int to)
		{
			return Ok(_stardustRepository.GetAllJobs(from, to));
		}

		[HttpGet, Route("Stardust/FailedJobs/{from}/{to}")]
		public IHttpActionResult FailedJobHistoryList(int from, int to)
		{
			return Ok(_stardustRepository.GetAllFailedJobs(from, to));
		}

		[HttpGet, Route("Stardust/RunningJobs")]
		public IHttpActionResult RunningJobsList()
		{
			return Ok(_stardustRepository.GetAllRunningJobs());
		}

		[HttpGet, Route("Stardust/JobsByNode/{nodeId}/{from}/{to}")]
		public IHttpActionResult JobHistoryList(Guid nodeId, int from, int to)
		{
			return Ok(_stardustRepository.GetJobsByNodeId(nodeId, from, to));
		}

		[HttpGet, Route("Stardust/JobDetails/{jobId}")]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			return Ok(_stardustRepository.GetJobDetailsByJobId(jobId));
		}

		[HttpGet, Route("Stardust/Job/{jobId}")]
		public IHttpActionResult Job(Guid jobId)
		{
			return Ok(_stardustRepository.GetJobByJobId(jobId));
		}

		[HttpGet, Route("Stardust/QueuedJobs/{from}/{to}")]
		public IHttpActionResult JobQueueList(int from, int to)
		{
			return Ok(_stardustRepository.GetAllQueuedJobs(from, to));
		}

		[HttpGet, Route("Stardust/QueuedJobs/{jobId}")]
		public IHttpActionResult QueuedJob(Guid jobId)
		{
			return Ok(_stardustRepository.GetQueuedJob(jobId));
		}

		[HttpGet, Route("Stardust/AliveWorkerNodes")]
		public IHttpActionResult AliveWorkerNodes()
		{
			var allNodes = _stardustRepository.GetAllWorkerNodes();
			return Ok(allNodes.Where(x => x.Alive));
		}

		[HttpGet, Route("Stardust/WorkerNodes")]
		public IHttpActionResult WorkerNodes()
		{
			return Ok(_stardustRepository.GetAllWorkerNodes());
		}

		[HttpGet, Route("Stardust/WorkerNode/{id}")]
		public IHttpActionResult WorkerNodes(Guid id)
		{
			return Ok(_stardustRepository.WorkerNode(id));
		}

		[HttpPost, Route("Stardust/DeleteQueuedJobs")]
		public IHttpActionResult DeleteQueuedJobs(Guid[] ids)
		{
			_stardustRepository.DeleteQueuedJobs(ids);
			return Ok();
		}

		[HttpPost, Route("Stardust/TriggerResourceCalculation")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult TriggerResourceCalculation([FromBody] LogOnModel logOnModel)
		{
			if (logOnModel == null) return BadRequest("logOnModel is null!");
			var tenant = _loadAllTenants.Tenants().Single(x => x.Name.Equals(logOnModel.Tenant));
			var appConnstring = tenant.DataSourceConfiguration.ApplicationConnectionString;
			var bus = _stardustRepository.SelectAllBus(appConnstring);

			foreach (var bu in bus)
			{
				_eventPublisher.Publish(
					new UpdateStaffingLevelReadModelEvent
					{
						Days = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 14),
						LogOnDatasource = logOnModel.Tenant,
						LogOnBusinessUnitId = bu
					});
			}

			return Ok();
		}

		[HttpGet, Route("Stardust/HealthCheck")]
		public IHttpActionResult HealthCheck()
		{
			var allNodes = _stardustRepository.GetAllWorkerNodes();
			if (!allNodes.Any())
				return InternalServerError(
					new Exception("No nodes registered! Make sure that the Teleopti Service Bus service is running."));
			if (!allNodes.Any(x => x.Alive))
				return InternalServerError(new Exception(
					"No node is sending heartbeats. Make sure that the Teleopti Service Bus service is running."));

			foreach (var node in allNodes.Where(x => x.Alive))
			{
				bool result;
				try
				{
					result = pingNode(node).Result;
				}
				catch (Exception)
				{
					return InternalServerError(new Exception(
						$"Node {node.Url} does not respond. Is the firewall configured so the worker server allows incoming traffic on ports 14100-14199?"));
				}
				if (!result)
					return InternalServerError(new Exception(
						$"Node {node.Url} does not respond. Is the firewall configured so the worker server allows incoming traffic on ports 14100-14199?"));
			}

			return Ok();
		}

		private static async Task<bool> pingNode(WorkerNode node)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var response = await client.PostAsync(node.Url + "ping/", null).ConfigureAwait(false);
				if (response.StatusCode != HttpStatusCode.OK)
				{
					return false;
				}
			}
			return true;
		}

	}

	public class LogOnModel
	{
		public string Tenant;
		public int Days;
	}
}
