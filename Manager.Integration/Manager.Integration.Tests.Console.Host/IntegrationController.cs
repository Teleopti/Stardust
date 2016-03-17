using System;
using System.Web.Http;
using log4net;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;

namespace Manager.IntegrationTest.Console.Host
{
	public class IntegrationController : ApiController
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (IntegrationController));

		public IntegrationController()
		{
			WhoAmI = "[INTEGRATION CONTROLLER, " + Environment.MachineName.ToUpper() + "]";

			Logger.LogInfoWithLineNumber(WhoAmI);
		}

		public string WhoAmI { get; set; }

		[HttpPost, Route("appdomain/managers")]
		public IHttpActionResult StartNewManager()
		{
			Logger.LogDebugWithLineNumber("Called API controller.");

			string friendlyname;

			Program.StartNewManager(out friendlyname);

			return Ok(friendlyname);
		}

		[HttpPost, Route("appdomain/nodes")]
		public IHttpActionResult StartNewNode()
		{
			Logger.LogInfoWithLineNumber("StartNewNode.");

			string friendlyname;

			Program.StartNewNode(out friendlyname);

			return Ok(friendlyname);
		}

		[HttpDelete, Route("appdomain/managers/{id}")]
		public IHttpActionResult DeleteManager(string id)
		{
			Logger.LogInfoWithLineNumber("DeleteManager");

			if (string.IsNullOrEmpty(id))
			{
				Logger.LogWarningWithLineNumber("Bad request, id : " + id);
				return BadRequest(id);
			}

			Logger.LogDebugWithLineNumber("Try shut down Manager with id : " + id);

			var success = Program.ShutDownManager(id);

			if (success)
			{
				Logger.LogInfoWithLineNumber("Manager has been shut down, with id : " + id);

				return Ok(id);
			}

			Logger.LogWarningWithLineNumber("Id not found, id : " + id);

			return NotFound();
		}

		[HttpDelete, Route("appdomain/nodes/{id}")]
		public IHttpActionResult DeleteNode(string id)
		{
			Logger.LogInfoWithLineNumber("Delete Node");

			if (string.IsNullOrEmpty(id))
			{
				Logger.LogWarningWithLineNumber("Bad request, id : " + id);
				return BadRequest(id);
			}

			Logger.LogDebugWithLineNumber("Try shut down Node with id : " + id);

			var success = Program.ShutDownNode(id);

			if (success)
			{
				Logger.LogInfoWithLineNumber("Node has been shut down, with id : " + id);

				return Ok(id);
			}

			Logger.LogWarningWithLineNumber("Id not found, id : " + id);

			return NotFound();
		}

		[HttpGet, Route("appdomain/managers")]
		public IHttpActionResult GetAllManagers()
		{
			Logger.LogDebugWithLineNumber("GetAllManagers");

			var appDomainsList = Program.GetAllmanagers();

			return Ok(appDomainsList);
		}

		[HttpGet, Route("appdomain/nodes")]
		public IHttpActionResult GetAllNodes()
		{
			Logger.LogDebugWithLineNumber("GetAllNodes");

			var appDomainsList = Program.GetAllNodes();

			return Ok(appDomainsList);
		}
	}
}