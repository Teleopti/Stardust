using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.Common.Logging
{
    public class AdvanceLoggingService
    {
        private static ILog _log;

        private static ILog getLog()
        {
            if (_log == null)
            {
                Log4NetConfiguration.SetConnectionString("data source=.;initial catalog=Log4net;integrated security=true; persist security info=True;User ID=sa;Password=cadadi");
                _log = LogManager.GetLogger("Teleopti.AdvanceLoggingService");
                
            }
            return _log;
        }

        public static void LogSchedulingInfo(ISchedulingOptions schedulingOptions, int noOfAgent, int noOfSkillDays, Action unknown)
        {
            if (getLog().IsInfoEnabled)
            {
                var stop = new Stopwatch();
                stop.Start();
                unknown.Invoke();
                stop.Stop();
                //get the system log
                populateSystemProperties();

                //log the scheduling options
                //populateSchedulingOptions(schedulingOptions);

                //log the agent and skill days
                populateAgentAndSkillDays(noOfAgent, noOfSkillDays);

                getLog().Info("Scheduling");
            }
        }

        private static void populateAgentAndSkillDays(int noOfAgent, int noOfSkillDays)
        {
            GlobalContext.Properties["NoOfAgents"] = noOfAgent.ToString();
            GlobalContext.Properties["NoOfSkillDays"] = noOfSkillDays.ToString();
        }

        private static void populateSystemProperties()
        {
            var identity = TeleoptiPrincipal.Current.Identity as ITeleoptiIdentity;
            if (identity != null)
            {
                GlobalContext.Properties["BU"] = identity.BusinessUnit.Name;
                //GlobalContext.Properties["BUId"] = identity.BusinessUnit.Id;
                GlobalContext.Properties["DataSource"] = identity.DataSource.DataSourceName;
                GlobalContext.Properties["InitialCatalog"] = identity.DataSource.InitialCatalog;
                GlobalContext.Properties["WindowsIdentity"] = identity.WindowsIdentity.Name;
                GlobalContext.Properties["HostIP"] = SystemInformationHelper.GetSystemIPAddress();
                GlobalContext.Properties["TeamScheduling"] = "team";
                GlobalContext.Properties["BlockScheduling"] = "team";
                GlobalContext.Properties["Scheduling"] = "Fairness: ";
            }
        }
    }
}
