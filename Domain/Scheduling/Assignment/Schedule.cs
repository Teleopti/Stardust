﻿using System;
using System.Linq;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public abstract class Schedule : ISchedule
    {
        private List<IBusinessRuleResponse> _businessRuleResponseCollection;
        private readonly IScheduleDictionary _owner;
        private readonly IScheduleParameters _parameters;
        private List<IScheduleData> _scheduleDataCollection;

		
        private readonly object lockObject = new object();
		
		private SchedulePublishedSpecification schedulePublishedSpecification;
		private SchedulePublishedSpecificationForAbsence schedulePublishedSpecificationForAbsence;
	    private SchedulePublishedSpecification schedulePublishedAnySpecification;

	    protected Schedule(IScheduleDictionary owner, IScheduleParameters parameters)
        {
            InParameter.NotNull("parameters", parameters);
            InParameter.NotNull("owner", owner);
            _owner = owner;
            _parameters = parameters;
            _businessRuleResponseCollection = new List<IBusinessRuleResponse>();
            _scheduleDataCollection = new List<IScheduleData>();

	        schedulePublishedSpecification = new SchedulePublishedSpecification(Person.WorkflowControlSet,
	                                                                            ScheduleVisibleReasons.Published);
			schedulePublishedAnySpecification =
					new SchedulePublishedSpecification(Person.WorkflowControlSet, ScheduleVisibleReasons.Any);
			schedulePublishedSpecificationForAbsence =
				new SchedulePublishedSpecificationForAbsence(Person.WorkflowControlSet, ScheduleVisibleReasons.Any);
        }

        public IScheduleDictionary Owner
        {
            get { return _owner; }
        }


        public DateTimePeriod Period
        {
            get { return _parameters.Period; }
        }

        public IPerson Person
        {
            get { return _parameters.Person; }
        }


        /// <summary>
        /// Gets the business rule response internal collection.
        /// </summary>
        /// <value>The business rule response internal collection.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-08-22    
        /// </remarks>
        public  IList<IBusinessRuleResponse> BusinessRuleResponseInternalCollection
        {
            get { return _businessRuleResponseCollection; }
        }


        protected IEnumerable<IScheduleData> ScheduleDataInternalCollection()
        {
            IEnumerable<IScheduleData> retList;
            lock(lockObject)
            {
                retList = _scheduleDataCollection.ToArray().AsParallel();
            }
            return retList;
        }

		protected IEnumerable<IPersistableScheduleData> PersistableScheduleDataInternalCollection()
        {
			return ScheduleDataInternalCollection().OfType<IPersistableScheduleData>();
        }

        public IScenario Scenario
        {
            get { return _parameters.Scenario; }
        }


        public virtual void Add(IScheduleData scheduleData)
        {
            if (checkData(scheduleData))
            {
                lock(lockObject)
                {
						 if(!_scheduleDataCollection.Contains(scheduleData))
									_scheduleDataCollection.Add(scheduleData);

                	var preferenceDay = scheduleData as IPreferenceDay;
					if (preferenceDay != null)
						removePreviousPrefenceForSameDay(preferenceDay);
                }
            }
        }

    	private void removePreviousPrefenceForSameDay(IPreferenceDay preferenceDay)
    	{
    		var preference =
    			_scheduleDataCollection.OfType<IPreferenceDay>().FirstOrDefault(
                    p => p.RestrictionDate == preferenceDay.RestrictionDate && preferenceDay != p);
			if (preference!=null)
			{
				_scheduleDataCollection.Remove(preference);
			}
    	}

    	public void AddRange<T>(IEnumerable<T> scheduleDataCollection) where T : IScheduleData
        {
            foreach (var scheduleData in scheduleDataCollection)
            {
                Add(scheduleData);
            }
        }

        public void AddRange(IEnumerable<IPersonAssignment> personAssignmentCollection)
        {
            lock(lockObject)
            {
                foreach (var assignment in personAssignmentCollection)
                {
						 if (checkData(assignment) && !_scheduleDataCollection.Contains(assignment))
                        _scheduleDataCollection.Add(assignment);
                }                
            }
        }

        public virtual void Remove(IScheduleData persistableScheduleData)
        {
            lock(lockObject)
            {
                _scheduleDataCollection.Remove(persistableScheduleData);
            }
        }

        public bool Contains(IScheduleData scheduleData)
        {
            return ScheduleDataInternalCollection().Contains(scheduleData);
        }

        /// <summary>
        /// Returns the total period of all IScheduledata, even if its not in the projection
        /// Returns null if empty
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-10-13
        /// </remarks>
        public DateTimePeriod? TotalPeriod()
        {
            IEnumerable<IScheduleData> scheduleDataClone = ScheduleDataInternalCollection();
            if (!scheduleDataClone.Any()) return null;


            return new DateTimePeriod(scheduleDataClone.Min(d => d.Period.StartDateTime), scheduleDataClone.Max(d => d.Period.EndDateTime));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected IScheduleDay ScheduleDay(IDateOnlyAsDateTimePeriod dateAndDateTime, bool includeUnpublished, IEnumerable<DateOnlyPeriod> availableDatePeriods)
        {
            IEnumerable<IScheduleData> filteredData;
            var period = dateAndDateTime.Period();
            //this will probably slow things down - fix later

            var schedIsPublished = schedulePublishedSpecification.IsSatisfiedBy(dateAndDateTime.DateOnly);
            var retObj = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(Owner, Person, dateAndDateTime);
            retObj.FullAccess = availableDatePeriods.Any(a => a.Contains(dateAndDateTime.DateOnly));
            retObj.IsFullyPublished = schedIsPublished;

            if (schedIsPublished || includeUnpublished)
            {
                filteredData = (from data in ScheduleDataInternalCollection()
                                where data.BelongsToPeriod(dateAndDateTime)
                                select (IScheduleData)data.Clone()).ToList();
            }
            else
            {
                var agentTimeZone = Person.PermissionInformation.DefaultTimeZone();

				filteredData = (from data in ScheduleDataInternalCollection()
                                where
                                    data.BelongsToPeriod(dateAndDateTime) &&
                                    (schedulePublishedAnySpecification.IsSatisfiedBy(
                                        new DateOnly(data.Period.StartDateTimeLocal(agentTimeZone)))
										|| schedulePublishedSpecificationForAbsence.IsSatisfiedBy(
										new PublishedScheduleData(dateAndDateTime, data, agentTimeZone)))
                                select (IScheduleData) data.Clone()).ToList();
            }
			foreach (var data in filteredData)
			{
				if (!retObj._scheduleDataCollection.Contains(data))
					retObj._scheduleDataCollection.Add(data);
			}
            BusinessRuleResponseInternalCollection.Where(rule => rule.Period.Contains(period.StartDateTime)).ForEach(retObj.BusinessRuleResponseInternalCollection.Add);
            return retObj;
        }

        private bool checkData(IScheduleData scheduleData)
        {
            InParameter.NotNull("scheduleData", scheduleData);
	      
            if(!scheduleData.Person.Equals(Person))
                throw new ArgumentOutOfRangeException("scheduleData", "Trying to add schedule info to incorrect person.");
            if (!scheduleData.BelongsToScenario(Scenario))
                throw new ArgumentOutOfRangeException("scheduleData", "Trying to add schedule info to incorrect scenario.");
            if (!WithinRange(scheduleData.Period))
            {
                if (!Owner.PermissionsEnabled)
                    return false; //if loaded from db, ignore stuff outside period
				////TODO check with RK, problems when splitting long absence periods
				//if (!(scheduleData is IPersonAbsence))
				//    throw new ArgumentOutOfRangeException(scheduleData.Period + " is outside " + Period + ". Cannot add to Schedule.");
            }

            return CheckPermission(scheduleData);
        }

        public bool WithinRange(DateTimePeriod period)
        {
            return Period.Intersect(period);
        }

        protected abstract bool CheckPermission(IScheduleData persistableScheduleData);

        public object Clone()
        {
            Schedule clone = (Schedule)MemberwiseClone();
            clone._businessRuleResponseCollection = new List<IBusinessRuleResponse>();
            clone._scheduleDataCollection = new List<IScheduleData>();
			  foreach (var data in ScheduleDataInternalCollection())
			  {
				  if (!clone._scheduleDataCollection.Contains(data))
					  clone._scheduleDataCollection.Add(data);
			  }
            _businessRuleResponseCollection.ForEach(clone.BusinessRuleResponseInternalCollection.Add);
            CloneDerived(clone);

            return clone;
        }
        protected virtual void CloneDerived(Schedule clone){}
    }

}
