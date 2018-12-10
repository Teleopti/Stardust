using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class PreferenceDay : VersionedAggregateRootWithBusinessUnit, IRestrictionOwner, IPreferenceDay, IAggregateRootWithEvents
    {
        private IPreferenceRestriction _restriction;
        private readonly IPerson _person;
        private readonly DateOnly _restrictionDate;
        private string _templateName;

        public PreferenceDay(IPerson person, DateOnly date, IPreferenceRestriction restriction)
        {
            _person = person;
            _restrictionDate = date;
            _restriction = restriction;
            restriction.SetParent(this);
        }

        protected PreferenceDay()
        {}

	    public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
		    base.NotifyTransactionComplete(operation);
			switch (operation)
			{
				case DomainUpdateType.Delete:
					AddEvent(new PreferenceDeletedEvent
					{
						PersonId = Person.Id.GetValueOrDefault(),
						RestrictionDates = new List<DateTime> {RestrictionDate.Date},
						Timestamp = ServiceLocatorForEntity.Now.UtcDateTime()
					});
					break;
				case DomainUpdateType.Insert:
					AddEvent(new PreferenceCreatedEvent
					{
						PersonId = Person.Id.GetValueOrDefault(),
						RestrictionDates = new List<DateTime> {RestrictionDate.Date},
						Timestamp = ServiceLocatorForEntity.Now.UtcDateTime()
					});
					break;
				case DomainUpdateType.Update:
					AddEvent(new PreferenceChangedEvent
					{
						PersonId = Person.Id.GetValueOrDefault(),
						RestrictionDates = new List<DateTime> {RestrictionDate.Date},
						Timestamp = ServiceLocatorForEntity.Now.UtcDateTime()
					});
					break;
			}
		}
		
        public virtual IPreferenceRestriction Restriction
        {
            get { return _restriction; }
        }

        public virtual IPerson Person
        {
            get { return _person; }
        }

        public virtual DateOnly RestrictionDate
        {
            get { return _restrictionDate; }
        }

        public virtual string TemplateName
        {
            get { return _templateName; }
            set { _templateName = value; }
        }

        #region IPersistableScheduleData Members

        public virtual string FunctionPath
        {
            get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction; }
        }

        public virtual IPersistableScheduleData CreateTransient()
        {
            IPreferenceDay ret = (IPreferenceDay)Clone();
            ret.SetId(null);
            ret.Restriction.SetId(null);
            return ret;
        }

        #endregion

        #region IScheduleData Members

        public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            return dateAndPeriod.DateOnly == _restrictionDate;
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            return dateOnlyPeriod.Contains(_restrictionDate);
        }

        public virtual bool BelongsToScenario(IScenario scenario)
        {
            return true;
        }
 
        #endregion

        #region IScheduleParameters Members


        public virtual IScenario Scenario
        {
            get { return null; }
        }

        #endregion

        #region IPeriodized Members

        public virtual DateTimePeriod Period
        {
            get
            {
                TimeSpan startTime = Restriction.StartTimeLimitation.StartTime.GetValueOrDefault(TimeSpan.Zero);
                DateTime agentLocalStart = DateTime.SpecifyKind(_restrictionDate.Date.Add(startTime), DateTimeKind.Unspecified);
                TimeSpan defaultEndTime = TimeSpan.FromHours(36);
                TimeSpan endTime = Restriction.EndTimeLimitation.EndTime.GetValueOrDefault(defaultEndTime);
                if (endTime < startTime)
                {
                    endTime = defaultEndTime;
                } 
                DateTime agentLocalEnd = DateTime.SpecifyKind(_restrictionDate.Date.Add(endTime), DateTimeKind.Unspecified);
                
                return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(agentLocalStart, agentLocalEnd, _person.PermissionInformation.DefaultTimeZone());
            }
        }

        #endregion

        #region ICloneable Members

        public virtual object Clone()
        {
            PreferenceDay clone = (PreferenceDay)MemberwiseClone();
            if(_restriction != null)
            {
                object restrictionClone =  ((PreferenceRestriction)_restriction).Clone();
                clone._restriction = (IPreferenceRestriction) restrictionClone;
                clone._restriction.SetParent(clone);
            }
            return clone;
        }

        #endregion

        #region IMainReference Members

        public virtual IAggregateRoot MainRoot
        {
            get { return Person; }
        }

        #endregion

        public virtual IEnumerable<IRestrictionBase> RestrictionBaseCollection
        {
            get { return new List<IRestrictionBase> {_restriction}; }
        }
    }
}
