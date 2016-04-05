using System;
using System.Web.Http;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class StardustController : ApiController
	{
		private readonly StardustRepository _stardustRepository;

		public StardustController(StardustRepository stardustRepository)
		{
			_stardustRepository = stardustRepository;
		}

		[HttpGet, Route("Stardust/JobHistoryList")]
		public IHttpActionResult JobHistoryList()
		{
			return Ok(_stardustRepository.HistoryList());
		}

		[HttpGet, Route("Stardust/NodeJobHistoryList/{nodeId}")]
		public IHttpActionResult JobHistoryList(Guid nodeId)
		{
			return Ok(_stardustRepository.HistoryList(nodeId));
		}

		[HttpGet, Route("Stardust/JobHistoryDetails/{jobId}")]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			return Ok(_stardustRepository.JobHistoryDetails(jobId));
		}

		[HttpGet, Route("Stardust/WorkerNodes")]
		public IHttpActionResult WorkerNodes()
		{
			return Ok(_stardustRepository.WorkerNodes());
		}

		[HttpGet, Route("Stardust/WorkerNode/{id}")]
		public IHttpActionResult WorkerNodes(Guid id)
		{
			return Ok(_stardustRepository.WorkerNode(id));
		}
	}
}
