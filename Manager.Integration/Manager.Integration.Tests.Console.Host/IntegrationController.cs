using System;
using System.Web.Http;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.IntegrationTest.Console.Host
{
	public class IntegrationController : ApiController
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (IntegrationController));

		public IntegrationController()
		{
			WhoAmI = "[INTEGRATION CONTROLLER, " + Environment.MachineName.ToUpper() + "]";

			LogHelper.LogInfoWithLineNumber(Logger,
			                                WhoAmI);
		}

		public string WhoAmI { get; set; }

		[HttpPost, Route("appdomain/managers")]
		public IHttpActionResult StartNewManager()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
											 "Called API controller.");

			string friendlyname;

			Program.StartNewManager(out friendlyname);

			return Ok(friendlyname);
		}

		[HttpPost, Route("appdomain/nodes")]
		public IHttpActionResult StartNewNode()
		{
			LogHelper.LogInfoWithLineNumber(Logger,
											 "StartNewNode.");

			string friendlyname;

			Program.StartNewNode(out friendlyname);

			return Ok(friendlyname);
		}

		[HttpDelete, Route("appdomain/managers/{id}")]
		public IHttpActionResult DeleteManager(string id)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
											 "DeleteManager");

			if (string.IsNullOrEmpty(id))
			{
				LogHelper.LogWarningWithLineNumber(Logger,
												   "Bad request, id : " + id);
				return BadRequest(id);
			}

			LogHelper.LogDebugWithLineNumber(Logger,
											"Try shut down Manager with id : " + id);

			var success = Program.ShutDownManager(id);

			if (success)
			{
				LogHelper.LogInfoWithLineNumber(Logger,
												"Manager has been shut down, with id : " + id);

				return Ok(id);
			}

			LogHelper.LogWarningWithLineNumber(Logger,
											   "Id not found, id : " + id);

			return NotFound();
		}

		[HttpDelete, Route("appdomain/nodes/{id}")]
		public IHttpActionResult DeleteNode(string id)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
			                                 "Delete Node");

			if (string.IsNullOrEmpty(id))
			{
				LogHelper.LogWarningWithLineNumber(Logger,
				                                   "Bad request, id : " + id);
				return BadRequest(id);
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                "Try shut down Node with id : " + id);

			var success = Program.ShutDownNode(id);

			if (success)
			{
				LogHelper.LogInfoWithLineNumber(Logger,
				                                "Node has been shut down, with id : " + id);

				return Ok(id);
			}

			LogHelper.LogWarningWithLineNumber(Logger,
			                                   "Id not found, id : " + id);

			return NotFound();
		}

		[HttpGet, Route("appdomain/managers")]
		public IHttpActionResult GetAllManagers()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
											 "GetAllManagers");

			var appDomainsList = Program.GetAllmanagers();

			return Ok(appDomainsList);
		}

		[HttpGet, Route("appdomain/nodes")]
		public IHttpActionResult GetAllNodes()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
											 "GetAllNodes");

			var appDomainsList = Program.GetAllNodes();

			return Ok(appDomainsList);
		}
	}
}