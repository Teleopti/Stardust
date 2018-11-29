using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class OvertimeAvailability : NonversionedAggregateRootWithBusinessUnit, IOvertimeAvailability, IDeleteTag
    {
        private readonly IPerson _person;
        private DateOnly _dateOfOvertime;
        private readonly TimeSpan? _startTime;
        private readonly TimeSpan? _endTime;
		private bool _isDeleted;


        public OvertimeAvailability(IPerson person, DateOnly dateOfOvertime, TimeSpan? startTime, TimeSpan? endTime)
        {
            _person = person;
            _dateOfOvertime = dateOfOvertime;
            _startTime = startTime;
            _endTime = endTime;
        }

        public virtual  DateTimePeriod Period
        {
            get
            {
                return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dateOfOvertime.Date,
                                                                            _dateOfOvertime.Date.AddDays(1),
                                                                            _person.PermissionInformation
                                                                                   .DefaultTimeZone());
            }
        }

        public virtual DateOnly DateOfOvertime { get { return _dateOfOvertime; } }

        protected OvertimeAvailability() { }

        public virtual IPerson Person { get { return _person; } }
        public virtual IScenario Scenario { get { return null; } }
        public virtual  object   Clone()
        {
            var clone = (OvertimeAvailability )MemberwiseClone();
            return clone;
        }

        public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            return dateAndPeriod.DateOnly == _dateOfOvertime ;
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            return dateOnlyPeriod.Contains(_dateOfOvertime );
        }

        public virtual bool BelongsToScenario(IScenario scenario)
        {
            return true;
        }

        public virtual IAggregateRoot MainRoot { get { return Person; } }

        public virtual string FunctionPath
        {
            get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction; }
        }

        public virtual IPersistableScheduleData CreateTransient()
        {
            var ret = (OvertimeAvailability )Clone();
            ret.SetId(null);
           
            return ret;
        }

        public virtual bool NotAvailable { get; set; }

        public virtual TimeSpan? StartTime { get { return _startTime; } }
        public virtual TimeSpan? EndTime { get { return _endTime ; } }

	    public virtual bool IsDeleted
	    {
		    get { return _isDeleted; }
	    }

	    public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
    }
}
