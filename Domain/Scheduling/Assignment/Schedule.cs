using System;
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
        private List<IPersonAssignment> _personAssignmentConflictCollection;
        private List<IScheduleData> _scheduleDataCollection;

        private readonly object lockObject = new object();

        protected Schedule(IScheduleDictionary owner, IScheduleParameters parameters)
        {
            InParameter.NotNull("parameters", parameters);
            InParameter.NotNull("owner", owner);
            _owner = owner;
            _parameters = parameters;
            _personAssignmentConflictCollection = new List<IPersonAssignment>();
            _businessRuleResponseCollection = new List<IBusinessRuleResponse>();
            _scheduleDataCollection = new List<IScheduleData>();
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

        /// <summary>
        /// Gets the conflicting assignment collection sorted by StartDateTime.
        /// </summary>
        protected IList<IPersonAssignment> PersonAssignmentConflictInternalCollection
        {
            get { return _personAssignmentConflictCollection; }
        }

        protected IEnumerable<IScheduleData> ScheduleDataInternalCollection()
        {
            IEnumerable<IScheduleData> retList;
            lock(lockObject)
            {
                retList = _scheduleDataCollection.ToArray();
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
                    _scheduleDataCollection.Add(scheduleData);

                    var pAss = scheduleData as IPersonAssignment;
                    if (pAss != null)
                        mightMoveAssignmentToConflictList(sortedAssignmentCollection(), pAss);

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
                    if (checkData(assignment))
                        _scheduleDataCollection.Add(assignment);
                }                
            }
            var assignmentCollection = sortedAssignmentCollection();
            for (var i = assignmentCollection.Count - 1; i >= 0; i--)
            {
                mightMoveAssignmentToConflictList(assignmentCollection, assignmentCollection[i]);
            }
        }

        private List<IPersonAssignment> sortedAssignmentCollection()
        {
            var assignmentCollection = new List<IPersonAssignment>(ScheduleDataInternalCollection().OfType<IPersonAssignment>());
            assignmentCollection.Sort(new PersonAssignmentByDateSorter());
            return assignmentCollection;
        }

        private void mightMoveAssignmentToConflictList(IList<IPersonAssignment> assignmentCollection, IPersonAssignment personAssignment)
        {
            var newIndex = assignmentCollection.IndexOf(personAssignment);
            if (assignmentCollection.Count > 1)
            {
                DateTimePeriod assPer = personAssignment.Period;
                if ((newIndex > 0 && assPer.StartDateTime < assignmentCollection[newIndex - 1].Period.EndDateTime) ||
                    (newIndex < assignmentCollection.Count - 1 && assPer.EndDateTime > assignmentCollection[newIndex + 1].Period.StartDateTime))
                {
                    _scheduleDataCollection.Remove(personAssignment);
                    assignmentCollection.Remove(personAssignment);
                    _personAssignmentConflictCollection.Add(personAssignment);
                }
            }
        }

        private void mightMoveAssignmentBackFromConflictList(IPersonAssignment ass)
        {
            if (_personAssignmentConflictCollection.Count > 0)
            {
                IPersonAssignment conflict=null;
                foreach (var assConflict in PersonAssignmentConflictInternalCollection)
                {
                    if(assConflict.Period.Intersect(ass.Period))
                    {
                        conflict = assConflict;
                        break;
                    }
                }
                if(conflict!=null)
                {
                    _personAssignmentConflictCollection.Remove(conflict);
                    Add(conflict);
                }
            }
        }

        public virtual void Remove(IScheduleData persistableScheduleData)
        {
            lock(lockObject)
            {
                var wasRemoved = _scheduleDataCollection.Remove(persistableScheduleData);

                var pAss = persistableScheduleData as IPersonAssignment;
                if (pAss != null)
                {
                    if(!_personAssignmentConflictCollection.Remove(pAss) && wasRemoved)
                        mightMoveAssignmentBackFromConflictList(pAss);
                }
            }
        }

        public void RemovePersonAssignment(IScheduleData persistableScheduleData)
        {
            lock (lockObject)
            {
                var pAss = persistableScheduleData as IPersonAssignment;

                if(pAss != null)
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
            if (scheduleDataClone.Count() == 0) return null;


            return new DateTimePeriod(scheduleDataClone.Min(d => d.Period.StartDateTime), scheduleDataClone.Max(d => d.Period.EndDateTime));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected IScheduleDay ScheduleDay(IDateOnlyAsDateTimePeriod dateAndDateTime, bool includeUnpublished, IEnumerable<DateOnlyPeriod> availableDatePeriods)
        {
            IEnumerable<IScheduleData> filteredData;
            IEnumerable<IPersonAssignment> filteredConflicts;
            var period = dateAndDateTime.Period();
            //this will probably slow things down - fix later

            SchedulePublishedSpecification schedulePublishedSpecification =
                new SchedulePublishedSpecification(Person.WorkflowControlSet, ScheduleVisibleReasons.Published);
            var schedIsPublished = schedulePublishedSpecification.IsSatisfiedBy(dateAndDateTime.DateOnly);
            var retObj = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(Owner, Person, dateAndDateTime);
            retObj.FullAccess = availableDatePeriods.Any(a => a.Contains(dateAndDateTime.DateOnly));
            retObj.IsFullyPublished = schedIsPublished;

            if (schedIsPublished || includeUnpublished)
            {
                filteredData = (from data in ScheduleDataInternalCollection()
                                where data.BelongsToPeriod(dateAndDateTime)
                                select (IScheduleData)data.Clone()).ToList();
                filteredConflicts = (from conflict in PersonAssignmentConflictInternalCollection
// ReSharper disable ImplicitlyCapturedClosure
									 // tamasb: note that we can ignore the R# warning here as both usages of schedulePublishedSpecification come   
									 // one after other quick, so the existance of schedulePublishedSpecification is not long
                                     where conflict.BelongsToPeriod(dateAndDateTime)
// ReSharper restore ImplicitlyCapturedClosure
                                     select conflict.EntityClone());
            }
            else
            {
                var agentTimeZone = Person.PermissionInformation.DefaultTimeZone();
                schedulePublishedSpecification =
                    new SchedulePublishedSpecification(Person.WorkflowControlSet, ScheduleVisibleReasons.Any);
				var schedulePublishedSpecificationForAbsence =
					new SchedulePublishedSpecificationForAbsence(Person.WorkflowControlSet, ScheduleVisibleReasons.Any, dateAndDateTime);
                filteredData = (from data in ScheduleDataInternalCollection()
                                where
                                    data.BelongsToPeriod(dateAndDateTime) &&
                                    (schedulePublishedSpecification.IsSatisfiedBy(
                                        new DateOnly(data.Period.StartDateTimeLocal(agentTimeZone)))
										|| schedulePublishedSpecificationForAbsence.IsSatisfiedBy(
                                        new PublishedScheduleData(data, agentTimeZone)))
                                select (IScheduleData) data.Clone()).ToList();
                filteredConflicts = (from conflict in PersonAssignmentConflictInternalCollection
                                     where
                                         conflict.BelongsToPeriod(dateAndDateTime) &&
									(schedulePublishedSpecification.IsSatisfiedBy(
										new DateOnly(conflict.Period.StartDateTimeLocal(agentTimeZone)))
										|| schedulePublishedSpecificationForAbsence.IsSatisfiedBy(
										new PublishedScheduleData(conflict, agentTimeZone)))
                                     select conflict.EntityClone());
            }
            filteredData.ForEach(retObj._scheduleDataCollection.Add);
            BusinessRuleResponseInternalCollection.Where(rule => rule.Period.Contains(period.StartDateTime)).ForEach(retObj.BusinessRuleResponseInternalCollection.Add);
            filteredConflicts.ForEach(retObj.PersonAssignmentConflictInternalCollection.Add);
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
                if((scheduleData is IPersonDayOff))
                    throw new DayOffOutsideScheduleException(scheduleData.Period + " is outside " + Period + ". Cannot add to Schedule.");
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
            clone._personAssignmentConflictCollection = new List<IPersonAssignment>();
            clone._scheduleDataCollection = new List<IScheduleData>();
            ScheduleDataInternalCollection().ForEach(clone._scheduleDataCollection.Add);
            _personAssignmentConflictCollection.ForEach(clone.PersonAssignmentConflictInternalCollection.Add);
            _businessRuleResponseCollection.ForEach(clone.BusinessRuleResponseInternalCollection.Add);
            CloneDerived(clone);

            return clone;
        }
        protected virtual void CloneDerived(Schedule clone){}
    }

}
