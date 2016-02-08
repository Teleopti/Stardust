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
        }

        [HttpGet, Route("appdomains")]
        public List<string> GetAllAppDomains()
        {
            LogHelper.LogInfoWithLineNumber(Logger, "Called API controller.");

            return Program.AppDomains.Keys.ToList();
        }
    }
}