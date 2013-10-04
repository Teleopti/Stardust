using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization;
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
                _log = LogManager.GetLogger("Teleopti.AdvanceLoggingService");
                
            }
            return _log;
        }

        public static void LogSchedulingInfo(ISchedulingOptions schedulingOptions, int noOfAgent, int noOfSkillDays, Action callbackAction)
        {
            var stop = new Stopwatch();
            stop.Start();
            callbackAction.Invoke();
            stop.Stop();

            if (getLog().IsInfoEnabled)
            {
				clearGlobalContext();
                
                //log the scheduling options
                populateSchedulingOptions(schedulingOptions);

                //log the agent and skill days
                populateAgentAndSkillDays(noOfAgent, noOfSkillDays,stop.ElapsedMilliseconds );

                getLog().Info("Scheduling");
            }
        }

        private static void populateSchedulingOptions(ISchedulingOptions schedulingOptions)
        {
            var schedulingOptionsValueExtractor = new SchedulingOptionsValueExtractor(schedulingOptions);
            getTeamBlockOptions(schedulingOptionsValueExtractor, schedulingOptions);
            //Scheduling
            GlobalContext.Properties["GeneralOptions"] = schedulingOptionsValueExtractor.GetGeneralSchedulingOptions();
        }

        private static void getTeamBlockOptions(SchedulingOptionsValueExtractor schedulingOptionsValueExtractor, ISchedulingOptions schedulingOptions)
        {

            if (schedulingOptions.UseTeamBlockPerOption && schedulingOptions.BlockFinderTypeForAdvanceScheduling != BlockFinderType.None)
            {
                GlobalContext.Properties["BlockOptions"] =
                    schedulingOptionsValueExtractor.GetBlockOptions();
            }
            else if (schedulingOptions.UseGroupScheduling)
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
            
            if (getLog().IsInfoEnabled)
            {
                clearGlobalContext();

                //log the optimization options
                populateOptimizationOptions(schedulingOptions);

                //log the agent and skill days
                populateAgentAndSkillDays(noOfAgent, noOfSkillDays, stopwatch.ElapsedMilliseconds);

                getLog().Info("Optimization");
            }
        }

        private static void clearGlobalContext()
        {
            GlobalContext.Properties["BlockOptions"] = "";
            GlobalContext.Properties["TeamOptions"] = "";
        }
    }
}
