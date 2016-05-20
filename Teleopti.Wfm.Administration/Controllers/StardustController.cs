using System;
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

		[HttpGet, Route("Stardust/Jobs")]
		public IHttpActionResult JobHistoryList()
		{
			return Ok(_stardustRepository.GetAllJobs());
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
