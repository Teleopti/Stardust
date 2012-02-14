using System;
using System.Collections.Generic;
using Teleopti.Ccc.DBConverter.GroupConverter;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DBConverter
{
    /// <summary>
    /// Tool for converting a 6x database into another database with new format
    /// </summary>
    public class DatabaseConverterClient
    {
        private readonly DateTimePeriod _period;
        private readonly ICccTimeZoneInfo _timeZoneInfo;
        private readonly DefaultAggregateRoot defaultAggregateRoot;
        private readonly MappedObjectPair mappedObjectPair;
        private readonly string _oldConnectionInformation;
        private int _defaultResolution;
        public event EventHandler<StatusEventArgs> StatusChanged;

        public DatabaseConverterClient(DateTime fromDate,
                                       DateTime toDate,
                                       DefaultAggregateRoot defaultAggregateRoots,
                                       ICccTimeZoneInfo timeZoneInfo,
                                       string oldConnectionInformation,
                                       int defaultResolution)
        {
            _period =
                new DateTimePeriod(
                    timeZoneInfo.ConvertTimeToUtc(fromDate, timeZoneInfo),
                    timeZoneInfo.ConvertTimeToUtc(toDate, timeZoneInfo));
            _timeZoneInfo = timeZoneInfo;
            _oldConnectionInformation = oldConnectionInformation;
            defaultAggregateRoot = defaultAggregateRoots;
            mappedObjectPair = new MappedObjectPair();
            _defaultResolution = defaultResolution;
        }

        public void StartConvert()
        {
            DateTime started = DateTime.Now;
            foreach (ModuleConverter converter in setupWhatModulesToRun())
            {
                converter.Convert();
            }

            DateTime ended = DateTime.Now;
            StatusChanged(this, new StatusEventArgs("Ready: Started " + started + "  Ended " + ended));
        }

        private IEnumerable<ModuleConverter> setupWhatModulesToRun()
        {
            //lägg villkor här vad man ska köra för nåt!
            IList<ModuleConverter> executeTheseModules = new List<ModuleConverter>
                 {
                 new CommonConverter(mappedObjectPair, _period, _timeZoneInfo, defaultAggregateRoot, _oldConnectionInformation),
                 new ForecastModuleConverter(mappedObjectPair, _period, _timeZoneInfo, _oldConnectionInformation, _defaultResolution),
                 new AgentModuleConverter(mappedObjectPair, _period, _timeZoneInfo),
                 new UserModuleConverter(mappedObjectPair, _period, _timeZoneInfo, defaultAggregateRoot),
                 new SecurityModuleConverter(mappedObjectPair, _period, _timeZoneInfo, defaultAggregateRoot),
                 new ScheduleModuleConverter(mappedObjectPair, _period, _timeZoneInfo, _oldConnectionInformation),
                 //new KpiModuleConverter(mappedObjectPair, _period, _timeZoneInfo), //These are in the default data now, 2009-05-12
                 new GroupingModuleConverter(mappedObjectPair, _period, _timeZoneInfo),
                 new EmployeeOptionalColumnModuleConverter(mappedObjectPair, _period, _timeZoneInfo)
                 };
            return executeTheseModules;
        }
    }
}
