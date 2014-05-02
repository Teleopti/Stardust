using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.Common.Logging
{
    public class AdvanceLoggingService
    {
		private static Lazy<ILog> _log = new Lazy<ILog>(() => LogManager.GetLogger("Teleopti.AdvanceLoggingService"));

        public static void LogSchedulingInfo(ISchedulingOptions schedulingOptions, int noOfAgent, int noOfSkillDays, Action callbackAction)
        {
            var stop = new Stopwatch();
            stop.Start();
            callbackAction.Invoke();
            stop.Stop();

            if (_log.Value.IsInfoEnabled)
            {
				clearGlobalContext();
                
                //log the scheduling options
                populateSchedulingOptions(schedulingOptions);

                //log the agent and skill days
                populateAgentAndSkillDays(noOfAgent, noOfSkillDays,stop.ElapsedMilliseconds );

                _log.Value.Info("Scheduling");
            }
        }

        private static void populateSchedulingOptions(ISchedulingOptions schedulingOptions)
        {
            var schedulingOptionsValueExtractor = new SchedulingOptionsValueExtractor(schedulingOptions);
            getTeamBlockOptions(schedulingOptionsValueExtractor, schedulingOptions);
            //Scheduling
            GlobalContext.Properties["GeneralOptions"] = schedulingOptionsValueExtractor.GetGeneralSchedulingOptions();
        }

	    private static void getTeamBlockOptions(SchedulingOptionsValueExtractor schedulingOptionsValueExtractor,
		    ISchedulingOptions schedulingOptions)
	    {

		    if (schedulingOptions.UseBlock &&
		        schedulingOptions.BlockFinderTypeForAdvanceScheduling != BlockFinderType.SingleDay)
		    {
			    GlobalContext.Properties["BlockOptions"] =
				    schedulingOptionsValueExtractor.GetBlockOptions();
		    }
			  else if (schedulingOptions.UseTeam)
		    {
			    GlobalContext.Properties["TeamOptions"] = schedulingOptionsValueExtractor.GetTeamOptions();
		    }
	    }

	    private static void populateOptimizationOptions(IOptimizationPreferences optimizationPreferences )
        {
            var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
            var schedulingOptionsValueExtractor = new SchedulingOptionsValueExtractor(schedulingOptions);
            getTeamBlockOptions(schedulingOptionsValueExtractor, schedulingOptions);
            //optimization
            GlobalContext.Properties["GeneralOptions"] = schedulingOptionsValueExtractor.GetGeneralOptimizationOptions(optimizationPreferences);
        }
        
        private static void populateAgentAndSkillDays(int noOfAgent, int noOfSkillDays, long elapsedMilliseconds)
        {
            GlobalContext.Properties["Agents"] = noOfAgent.ToString();
            GlobalContext.Properties["SkillDays"] = noOfSkillDays.ToString();
            GlobalContext.Properties["ExecutionTime"] = elapsedMilliseconds.ToString();
            populateSystemProperties();
        }

        private static void populateSystemProperties()
        {
            var identity = TeleoptiPrincipal.Current.Identity as ITeleoptiIdentity;
            if (identity != null)
            {
                GlobalContext.Properties["BU"] = identity.BusinessUnit.Name;
                GlobalContext.Properties["BUId"] = identity.BusinessUnit.Id;
                GlobalContext.Properties["DataSource"] = identity.DataSource.DataSourceName;
                GlobalContext.Properties["WindowsIdentity"] = identity.WindowsIdentity.Name;
                GlobalContext.Properties["HostIP"] = SystemInformationHelper.GetSystemIPAddress();
                
            }
        }

        public static void LogOptimizationInfo(IOptimizationPreferences schedulingOptions, int noOfAgent, int noOfSkillDays, Action callbackAction)
        {

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            callbackAction.Invoke();
            stopwatch.Stop(); 
            
            if (_log.Value.IsInfoEnabled)
            {
                clearGlobalContext();

                //log the optimization options
                populateOptimizationOptions(schedulingOptions);

                //log the agent and skill days
                populateAgentAndSkillDays(noOfAgent, noOfSkillDays, stopwatch.ElapsedMilliseconds);

                _log.Value.Info("Optimization");
            }
        }

        private static void clearGlobalContext()
        {
            GlobalContext.Properties["BlockOptions"] = "";
            GlobalContext.Properties["TeamOptions"] = "";
        }
    }
}
