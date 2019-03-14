using System.Web.Http;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class StatusController : ApiController
	{
		private readonly ListStatusSteps _listStatusSteps;

		public StatusController(ListStatusSteps listStatusSteps)
		{
			_listStatusSteps = listStatusSteps;
		}
		
		//duplicate of statuscontroller in web. remove this (or web's) endpoint later...
		[HttpGet]
		[Route("status/list")]
		public IHttpActionResult List()
		{
			return Ok(_listStatusSteps.Execute());
		}
	}
}