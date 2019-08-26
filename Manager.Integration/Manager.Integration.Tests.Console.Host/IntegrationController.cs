using System;
using System.Collections.Generic;
using System.Web.Http;
using Manager.IntegrationTest.Console.Host.Constants;
using Manager.IntegrationTest.Console.Host.Log4Net;

namespace Manager.IntegrationTest.Console.Host
{
	public class IntegrationController : ApiController
	{
		public IntegrationController()
		{
			WhoAmI = "[INTEGRATION CONTROLLER, " + Environment.MachineName.ToUpper() + "]";

			this.Log().InfoWithLineNumber(WhoAmI);
		}

		public string WhoAmI { get; set; }

		[HttpPost, Route(IntegrationControllerRouteConstants.Managers)]
		public IHttpActionResult StartNewManager()
		{
			this.Log().DebugWithLineNumber("Called API controller.");

            Program.StartNewManager(out var friendlyName);

			return Ok(friendlyName);
		}

		[HttpPost, Route(IntegrationControllerRouteConstants.Nodes)]
		public IHttpActionResult StartNewNode()
		{
			this.Log().InfoWithLineNumber("StartNewNode.");

            Program.StartNewNode(out var friendlyName);

			return Ok(friendlyName);
		}

		[HttpDelete, Route(IntegrationControllerRouteConstants.ManagerById)]
		public IHttpActionResult DeleteManager(string id)
		{
			this.Log().InfoWithLineNumber("DeleteManager");

			if (string.IsNullOrEmpty(id))
			{
				this.Log().WarningWithLineNumber("Bad request, id : " + id);
				return BadRequest(id);
			}

			this.Log().DebugWithLineNumber("Try shut down Manager with id : " + id);

			var success = Program.ShutDownManager(id);

			if (success)
			{
				this.Log().InfoWithLineNumber("Manager has been shut down, with id : " + id);

				return Ok(id);
			}

			this.Log().WarningWithLineNumber("Id not found, id : " + id);

			return NotFound();
		}

		[HttpDelete, Route(IntegrationControllerRouteConstants.NodeById)]
		public IHttpActionResult DeleteNode(string id)
		{
			this.Log().InfoWithLineNumber("Delete Node");

			if (string.IsNullOrEmpty(id))
			{
				this.Log().WarningWithLineNumber("Bad request, id : " + id);
				return BadRequest(id);
			}

			this.Log().DebugWithLineNumber("Try shut down Node with id : " + id);

			var success = Program.ShutDownNode(id);

			if (success)
			{
				this.Log().InfoWithLineNumber("Node has been shut down, with id : " + id);

				return Ok(id);
			}

			this.Log().WarningWithLineNumber("Id not found, id : " + id);

			return NotFound();
		}

		[HttpGet, Route(IntegrationControllerRouteConstants.Managers)]
		public IHttpActionResult GetAllManagers()
		{
			this.Log().DebugWithLineNumber("GetAllManagers");

			List<string> appDomainsList = Program.GetAllManagers();

			return Ok(appDomainsList);
		}

		[HttpGet, Route(IntegrationControllerRouteConstants.Nodes)]
		public IHttpActionResult GetAllNodes()
		{
			this.Log().DebugWithLineNumber("GetAllNodes");

			var appDomainsList = Program.GetAllNodes();

			return Ok(appDomainsList);
		}
	}
}