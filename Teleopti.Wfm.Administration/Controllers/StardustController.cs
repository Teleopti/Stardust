using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.Administration.Controllers
{
    [TenantTokenAuthentication]
    public class StardustController : ApiController
	{
		private readonly StardustRepository _stardustRepository;

		public StardustController(StardustRepository stardustRepository)
		{
			_stardustRepository = stardustRepository;
		}

		[HttpGet, Route("Stardust/Jobs/{from}/{to}")]
		public IHttpActionResult JobHistoryList(int from, int to)
		{
			return Ok(_stardustRepository.GetAllJobs(from, to));
		}

		[HttpGet, Route("Stardust/RunningJobs")]
		public IHttpActionResult RunningJobsList()
		{
			return Ok(_stardustRepository.GetAllRunningJobs());
		}

		[HttpGet, Route("Stardust/JobsByNode/{nodeId}")]
		public IHttpActionResult JobHistoryList(Guid nodeId)
		{
			return Ok(_stardustRepository.GetJobsByNodeId(nodeId));
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
	}
}
