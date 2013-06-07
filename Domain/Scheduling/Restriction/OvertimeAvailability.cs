using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public interface IOvertimeAvailability : IPersistableScheduleData
    {
        bool NotAvailable { get; set; }
    }

    public class OvertimeAvailability : AggregateRootWithBusinessUnit, IOvertimeAvailability
    {
        private readonly IPerson _person;
        private DateOnly _overtimeDate;
        private readonly TimeSpan? _startTime;
        private readonly TimeSpan? _endTime;


        public OvertimeAvailability(IPerson person, DateOnly overtimeDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            _person = person;
            _overtimeDate = overtimeDate;
            _startTime = startTime;
            _endTime = endTime;
        }

        public DateTimePeriod Period
        {
            get
            {
                return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_overtimeDate.Date,
                                                                            _overtimeDate.Date.AddDays(1),
                                                                            _person.PermissionInformation
                                                                                   .DefaultTimeZone());
            }
        }

        public IPerson Person { get { return _person; } }
        public IScenario Scenario { get { return null; } }
        public object   Clone()
        {
            var clone = (OvertimeAvailability )MemberwiseClone();
            return clone;
        }

        public bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            return dateAndPeriod.DateOnly == _overtimeDate ;
        }

        public bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            return dateOnlyPeriod.Contains(_overtimeDate );
        }

        public bool BelongsToScenario(IScenario scenario)
        {
            return true;
        }

        public IAggregateRoot MainRoot { get { return Person; } }

        public string FunctionPath
        {
            get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction; }
        }

        public IPersistableScheduleData CreateTransient()
        {
            var ret = (OvertimeAvailability )Clone();
            ret.SetId(null);
           
            return ret;
        }

        public bool NotAvailable { get; set; }

        public TimeSpan? StartTime { get { return _startTime; } }
        public TimeSpan? EndTime { get { return _endTime ; } }
    }
}
