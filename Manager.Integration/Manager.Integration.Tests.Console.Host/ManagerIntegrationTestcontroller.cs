using System;
using System.Collections.Generic;
using System.Web.Http;
using log4net;

namespace Manager.IntegrationTest.Console.Host
{
    public class ManagerIntegrationTestController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (ManagerIntegrationTestController));

        public string WhoAmI { get; private set; }

        public ManagerIntegrationTestController()
        {
            WhoAmI = "[MANAGER INTEGRATION TEST CONTROLLER, " + Environment.MachineName.ToUpper() + "]";
        }

        [HttpGet, Route("appdomains")]
        public IEnumerable<AppDomain> GetAllAppDomains()
        {
            return Program.AppDomains.Values;
        }
    }
}