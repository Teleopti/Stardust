using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.IntegrationTest.Console.Host
{
    public class ManagerIntegrationTestController : ApiController
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (ManagerIntegrationTestController));

        public string WhoAmI { get; private set; }

        public ManagerIntegrationTestController()
        {
            WhoAmI = "[MANAGER INTEGRATION TEST CONTROLLER, " + Environment.MachineName.ToUpper() + "]";

            LogHelper.LogInfoWithLineNumber(Logger,
                                            WhoAmI);
        }

        [HttpPost, Route("appdomain")]
        public void StartNewNode()
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Called API controller.");

            Program.StartNewNode();
        }

        [HttpDelete, Route("appdomain/{id}")]
        public IHttpActionResult DeleteAppDomain(string id)
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Called API controller.");

            if (string.IsNullOrEmpty(id))
            {
                LogHelper.LogWarningWithLineNumber(Logger,
                                                   "Bad request, id : " + id);

                return BadRequest(id);
            }

            bool nodeExist = Program.NodeExists(id);

            if (nodeExist)
            {
                LogHelper.LogWarningWithLineNumber(Logger,
                                                   "Shut down Node with id : " + id);

                Program.ShutDownNode(id);

                return Ok(id);
            }

            LogHelper.LogWarningWithLineNumber(Logger,
                                               "Id not found, id : " + id);

            return NotFound();
        }

        [HttpGet, Route("appdomain")]
        public IHttpActionResult GetAllAppDomains()
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Called API controller.");

            List<string> appDomainsList = Program.GetInstantiatedAppDomains();

            return Ok(appDomainsList);
        }
    }
}