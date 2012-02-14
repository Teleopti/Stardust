using System;
using System.Collections.Specialized;
using System.Threading;
using log4net;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DBConverter.GroupConverter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DBConverter
{
    public class DatabaseConvert
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseConvert));
        private DatabaseConverterClient converterClient;
        private readonly Initialize _init;
        private readonly ICommandLineArgument _argument;

        public DatabaseConvert(ICommandLineArgument argument)
        {
            _argument = argument;
            _init = new Initialize();
            _init.CreateOldDomainAndLogOn(new StateOldVersion(), argument.SourceServer, argument.SourceDatabase, 30);
        }

        public void StartConverter(DateTime fromDate, DateTime toDate, ICccTimeZoneInfo timeZone, DefaultAggregateRoot defaultAggregateRoot, int defaultResolution)
        {
            converterClient = new DatabaseConverterClient(fromDate, toDate, defaultAggregateRoot, timeZone, _argument.SourceConnectionString, defaultResolution);
            converterClient.StatusChanged += converter_StatusChanged;
            converterClient.StartConvert();
        }

        private static void converter_StatusChanged(object sender, StatusEventArgs e)
        {
            Logger.Info(e.StatusText);
        }

        public static bool CheckRaptorCompatibility()
        {
            global::Infrastructure.RaptorCompabilityReader reader = new global::Infrastructure.RaptorCompabilityReader();
            StringCollection compabilityIssues = reader.GetIssueList();

            if (compabilityIssues.Count > 0)
            {
                foreach (string issue in compabilityIssues)
                {
                    Logger.Info(issue);
                }
                Thread.Sleep(5000);
                return false;
            }
            return true;
        }

        public void MergeToResolution(
            DateTime fromDate, 
            DateTime toDate, 
            ICccTimeZoneInfo timeZone, 
            int defaultResolution)
        {
            ForecastModuleConverter forecastModuleConverter = new ForecastModuleConverter(
                new MappedObjectPair(),
                new DateTimePeriod(
                    timeZone.ConvertTimeToUtc(fromDate, timeZone),
                    timeZone.ConvertTimeToUtc(toDate, timeZone)),
                timeZone,
                _argument.SourceConnectionString,
                defaultResolution);
            forecastModuleConverter.MergeToNewDefaultResolution();
        }
    }
}
