using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.Administration.Controllers
{
    public class StardustController : ApiController
    {
	    private readonly StardustHelper _stardustHelper;

	    public StardustController(StardustHelper stardustHelper)
	    {
		    _stardustHelper = stardustHelper;
	    }

	    [HttpGet, Route("Stardust/JobHistoryList")]
		public IHttpActionResult JobHistoryList()
		{
			return Ok(_stardustHelper.GetJobHistoryList());
		}

		[HttpGet, Route("Stardust/NodeJobHistoryList/{nodeId}")]
		public IHttpActionResult JobHistoryList(Guid nodeId)
		{
			return Ok(_stardustHelper.GetJobHistoryList(nodeId));
		}

		[HttpGet, Route("Stardust/JobHistory/{jobId}")]
		public IHttpActionResult JobHistory(Guid jobId)
		{
			var jobHistory = _stardustHelper.GetJobHistory(jobId);

			return Ok(jobHistory);
		}

		[HttpGet, Route("Stardust/JobHistoryDetails/{jobId}")]
		public IHttpActionResult JobHistoryDetails(Guid jobId)
		{
			return Ok(_stardustHelper.JobHistoryDetails(jobId));
		}

		[HttpGet, Route("Stardust/WorkerNodes")]
	    public IHttpActionResult WorkerNodes()
	    {
		    return Ok(_stardustHelper.WorkerNodes());
	    }

		[HttpGet, Route("Stardust/WorkerNode/{Id}")]
		public IHttpActionResult WorkerNodes(Guid Id)
		{
			return Ok(_stardustHelper.WorkerNode(Id));
		}

	}
}
