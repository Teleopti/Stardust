using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.Administration.Controllers
{
    [TenantTokenAuthentication]
    public class StardustController : ApiController
	{
		private readonly StardustRepository _stardustRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IJobStartTimeRepository _jobStartTimeRepository;

		public StardustController(StardustRepository stardustRepository, IEventPublisher eventPublisher, 
			ILoadAllTenants loadAllTenants, IJobStartTimeRepository jobStartTimeRepository)
		{
			_stardustRepository = stardustRepository;
			_eventPublisher = eventPublisher;
			_loadAllTenants = loadAllTenants;
			_jobStartTimeRepository = jobStartTimeRepository;
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
			if(logOnModel == null) return BadRequest("logOnModel is null!");
			var tenant = _loadAllTenants.Tenants().Single(x => x.Name.Equals(logOnModel.Tenant));
			var appConnstring = tenant.DataSourceConfiguration.ApplicationConnectionString;
			var bus = _stardustRepository.SelectAllBus(appConnstring);
			
			foreach (var bu in bus)
			{
				if (logOnModel.Days == 1)
					_eventPublisher.Publish(
						new UpdateStaffingLevelReadModelEvent
						{
							Days = logOnModel.Days,
							LogOnDatasource = logOnModel.Tenant,
							LogOnBusinessUnitId = bu
						});
				else
					_eventPublisher.Publish(
						new UpdateStaffingLevelReadModel2WeeksEvent
						{
							Days = 14,
							LogOnDatasource = logOnModel.Tenant,
							LogOnBusinessUnitId = bu
						}
					);
			}

			return Ok();
		}


	}

	public class LogOnModel
	{
		public string Tenant;
		public int Days;
	}
}
