using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for meetings
    /// </summary>
    public class MeetingRepository : Repository<IMeeting>, IMeetingRepository
    {
				public MeetingRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Find by period, scenario
        /// </summary>
        /// <param name="period"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public ICollection<IMeeting> Find(DateTimePeriod period, IScenario scenario)
        {
            InParameter.NotNull(nameof(period), period);
            InParameter.NotNull(nameof(scenario), scenario);

            period = period.ChangeStartTime(TimeSpan.FromDays(-1)).ChangeEndTime(TimeSpan.FromDays(1)); //To compensate for time zones
            IList<IMeeting> retList = Session.CreateCriteria(typeof(Meeting), "ME")
                .Add(Restrictions.Le("StartDate", new DateOnly(period.EndDateTime)))
                .Add(Restrictions.Gt("EndDate", new DateOnly(period.StartDateTime)))
                .Add(Restrictions.Eq("Scenario", scenario))
                .SetFetchMode("MeetingPersons", FetchMode.Join)
                .SetFetchMode("meetingRecurrenceOptions", FetchMode.Join)
                .SetFetchMode("meetingRecurrenceOptions.WeekDays", FetchMode.Join)
                .SetFetchMode("Activity", FetchMode.Join)
                .SetFetchMode("Organizer", FetchMode.Join)
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IMeeting>();

            return retList;
        }

        public ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, bool includeForOrganizer = true)
        {
    		InParameter.NotNull(nameof(persons), persons);
    		InParameter.NotNull(nameof(period), period);
    		InParameter.NotNull(nameof(scenario), scenario);

    		var retList = new List<IMeeting>();

    		period = period.Inflate(1); //To compensate for time zones
    		foreach (var personList in persons.Batch(400))
    		{
    			var people = personList.ToArray();
    			AbstractCriterion personRestriction = Subqueries.PropertyIn("Id", DetachedCriteria.For<MeetingPerson>()
    			                                                                  	.Add(Restrictions.InG("Person", people))
    			                                                                  	.SetProjection(Projections.Property("Parent")));
    			if (includeForOrganizer)
    			{
    				personRestriction = Restrictions.Or(Restrictions.InG("Organizer", people), personRestriction);
    			}

    			var tempList = Session.CreateCriteria<Meeting>()
    				.Add(Restrictions.Le("StartDate", period.EndDate))
    				.Add(Restrictions.Gt("EndDate", period.StartDate))
    				.Add(Restrictions.Eq("Scenario", scenario))
    				.Add(personRestriction)
    				.SetFetchMode("MeetingPersons", FetchMode.Join)
    				.SetFetchMode("meetingRecurrenceOptions", FetchMode.Join)
    				.SetFetchMode("meetingRecurrenceOptions.WeekDays", FetchMode.Join)
    				.SetFetchMode("Activity", FetchMode.Join)
    				.SetFetchMode("Organizer", FetchMode.Join)
    				.SetResultTransformer(Transformers.DistinctRootEntity)
    				.List<IMeeting>();

				retList.AddRange(tempList);
			}

    		return retList.Distinct().ToArray();
    	}
		
        public IMeeting LoadAggregate(Guid id)
        {
            IMeeting meeting = Get(id);
	        if (meeting != null)
	        {
		        if (!LazyLoadingManager.IsInitialized(meeting.Activity))
			        LazyLoadingManager.Initialize(meeting.Activity);
		        if (!LazyLoadingManager.IsInitialized(meeting.Organizer))
			        LazyLoadingManager.Initialize(meeting.Organizer);
		        if (!LazyLoadingManager.IsInitialized(meeting.Scenario))
			        LazyLoadingManager.Initialize(meeting.Scenario);
		        foreach (IMeetingPerson person in meeting.MeetingPersons)
		        {
			        LazyLoadingManager.Initialize(person);
			        if (!LazyLoadingManager.IsInitialized(person.Person))
			        {
				        LazyLoadingManager.Initialize(person.Person);
				        foreach (IPersonPeriod personPeriod in person.Person.PersonPeriodCollection)
				        {
					        if (!LazyLoadingManager.IsInitialized(personPeriod))
						        LazyLoadingManager.Initialize(personPeriod);
					        if (!LazyLoadingManager.IsInitialized(personPeriod.Team))
						        LazyLoadingManager.Initialize(personPeriod.Team);
				        }
			        }
		        }
	        }
	        return meeting;
        }

        
        public IList<IMeeting> FindMeetingsWithTheseOriginals(ICollection<IMeeting> meetings, IScenario scenario)
        {
            var guidList = meetings.Select(p => p.Id.GetValueOrDefault());
            var result = new List<IMeeting>();
            foreach (var meetingsBatch in guidList.Batch(200))
            {
                var currentBatchIds = meetingsBatch.ToArray();

                result.AddRange(Session.CreateCriteria(typeof(IMeeting))
                    .Add(Restrictions.In("OriginalMeetingId", currentBatchIds))
                    .Add(Restrictions.Eq("Scenario", scenario))
                    .SetResultTransformer(Transformers.DistinctRootEntity)
                    .List<IMeeting>());
                
            }
            return result;
        }
    }
}
