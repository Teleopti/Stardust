using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class StatusController : ApiController
	{
		private readonly ListStatusSteps _listStatusSteps;
		private readonly IConfigReader _configReader;

		public StatusController(ListStatusSteps listStatusSteps, IConfigReader configReader)
		{
			_listStatusSteps = listStatusSteps;
			_configReader = configReader;
		}
		
		//duplicate of statuscontroller in web. remove this (or web's) endpoint later...
		[HttpGet]
		[Route("status/list")]
		public IHttpActionResult List()
		{
			return Ok(_listStatusSteps.Execute(new Uri(_configReader.AppConfig("Settings")), "status"));
		}
	}
}