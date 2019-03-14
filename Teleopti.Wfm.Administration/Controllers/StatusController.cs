using System.Web.Http;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class StatusController : ApiController
	{
		private readonly ListCustomStatusSteps _listCustomStatusSteps;

		public StatusController(ListCustomStatusSteps listCustomStatusSteps)
		{
			_listCustomStatusSteps = listCustomStatusSteps;
		}
		
		[HttpGet]
		[Route("status/listCustom")]
		public IHttpActionResult ListCustom()
		{
			return Ok(_listCustomStatusSteps.Execute());
		}
	}
}