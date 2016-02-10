using System;
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
        }

        [HttpPost, Route("appdomain")]
        public void StartNewNode()
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Called API controller.");

            Program.StartNewNode();
        }

        [HttpDelete, Route("appdomain/{id}")]
        public IHttpActionResult DeleteAppDomain(string id)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Called API controller.");

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(id);
            }

            bool nodeExist = Program.NodeExists(id);

            if (nodeExist)
            {
                Program.ShutDownNode(id);

                return Ok(id);
            }
            else
            {
                return NotFound();
            }                       
        }

        [HttpGet, Route("appdomain")]
        public IHttpActionResult GetAllAppDomains()
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "Called API controller.");

            return Ok(Program.AppDomains.Keys.ToList());
        }
    }
}