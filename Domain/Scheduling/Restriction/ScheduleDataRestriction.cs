using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class ScheduleDataRestriction : IScheduleDataRestriction, IRestrictionOwner
    {
        private readonly IPerson _person;
        private readonly DateOnly _restrictionDate;


        public ScheduleDataRestriction(IPerson person ,IRestrictionBase restriction, DateOnly date)
        {
            _person = person;
            Restriction = restriction;
            _restrictionDate = date;
        }

        public IRestrictionBase Restriction { get; private set; }

        public DateOnly RestrictionDate
        {
            get { return _restrictionDate;}
        }


        public DateTimePeriod Period
        {
            get
            {
                TimeSpan startTime;
                if (Restriction.StartTimeLimitation.StartTime.HasValue)
                    startTime = Restriction.StartTimeLimitation.StartTime.Value;
                else
                {
                    startTime = TimeSpan.Zero;
                }
                DateTime agentLocalStart = DateTime.SpecifyKind(_restrictionDate.Date.Add(startTime), DateTimeKind.Unspecified);
                TimeSpan endTime;
                if (Restriction.EndTimeLimitation.EndTime.HasValue)
                    endTime = Restriction.EndTimeLimitation.EndTime.Value;
                else
                {
                    endTime = TimeSpan.FromHours(36);
                }
                DateTime agentLocalEnd = DateTime.SpecifyKind(_restrictionDate.Date.Add(endTime), DateTimeKind.Unspecified);

                return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(agentLocalStart, agentLocalEnd, _person.PermissionInformation.DefaultTimeZone());
            }
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public IScenario Scenario
        {
            get { return null; }
        }


        public object Clone()
        {
            ScheduleDataRestriction clone = (ScheduleDataRestriction)MemberwiseClone();
 
            return clone;
        }


        public bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            return dateAndPeriod.DateOnly == _restrictionDate;
        }

        public bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            return dateOnlyPeriod.Contains(_restrictionDate);
        }

        public bool BelongsToScenario(IScenario scenario)
        {
            return true; 
        }



        public virtual bool IsAvailabilityRestriction
        {
            get
            {
                return typeof(IAvailabilityRestriction).IsInstanceOfType(Restriction) ||
                       typeof(StudentAvailabilityRestriction).IsInstanceOfType(Restriction);
            }
        }
        public virtual bool IsRotationRestriction
        {
            get
            {
                bool isType = new RotationSpecification().IsSatisfiedBy(Restriction);
                return (isType && Restriction.IsRestriction());
            }
        }

        public virtual bool IsPreferenceRestriction
        {
            get
            {
                return typeof(IPreferenceRestriction).IsInstanceOfType(Restriction);
            }
        }

        public IEnumerable<IRestrictionBase> RestrictionBaseCollection
        {
            get { return new List<IRestrictionBase> {Restriction}; }
        }
    }
}