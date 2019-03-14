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
		
		[HttpGet]
		[Route("status/listCustom")]
		public IHttpActionResult ListCustom()
		{
			return Ok(_listStatusSteps.Execute(false));
		}
	}
}