﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for meetings
    /// </summary>
    public class MeetingRepository : Repository<IMeeting>, IMeetingRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public MeetingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public MeetingRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
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
            InParameter.NotNull("period", period);
            InParameter.NotNull("scenario", scenario);

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

        public ICollection<IMeeting> Find(IEnumerable<Guid> persons, DateOnlyPeriod period, IScenario scenario, bool includeForOrganizer)
        {
            InParameter.NotNull("persons", persons);
            InParameter.NotNull("period", period);
            InParameter.NotNull("scenario", scenario);

            ICollection<IMeeting> retList = new HashSet<IMeeting>();

			period = new DateOnlyPeriod(period.StartDate.AddDays(-1),period.EndDate.AddDays(1)); //To compensate for time zones
			foreach (var personList in persons.Batch(400))
			{
				IList<IMeeting> tempList = Session.CreateCriteria(typeof (Meeting), "ME")
					.Add(Restrictions.Le("StartDate", period.EndDate))
					.Add(Restrictions.Gt("EndDate", period.StartDate))
					.Add(Restrictions.Eq("Scenario", scenario))
					.SetFetchMode("MeetingPersons", FetchMode.Join)
					.SetFetchMode("meetingRecurrenceOptions", FetchMode.Join)
					.SetFetchMode("meetingRecurrenceOptions.WeekDays", FetchMode.Join)
					.SetFetchMode("Activity", FetchMode.Join)
					.SetFetchMode("Organizer", FetchMode.Join)
					.SetResultTransformer(Transformers.DistinctRootEntity)
					.List<IMeeting>();

				foreach (IMeeting meeting in tempList)
				{
					if (includeForOrganizer && personList.Contains(meeting.Organizer.Id.GetValueOrDefault()))
					{
						retList.Add(meeting);
					}
					else
					{
						foreach (IMeetingPerson meetingPerson in meeting.MeetingPersons)
						{
							if (personList.Contains(meetingPerson.Person.Id.GetValueOrDefault()))
							{
								retList.Add(meeting);
							}
						}
					}
				}
			}

        	return retList;
        }

        public ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, bool includeForOrganizer)
		{
            var peopleId = persons.Select(p => p.Id.GetValueOrDefault());
            return Find(peopleId, period, scenario, includeForOrganizer);
		}

        public ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
        {
        	return Find(persons, period, scenario, true);
            
        }

        public IMeeting LoadAggregate(Guid id)
        {
            IMeeting meeting = Get(id);
            if(meeting!=null)
            {
                LazyLoadingManager.Initialize(meeting.Activity);
                LazyLoadingManager.Initialize(meeting.Organizer);
                LazyLoadingManager.Initialize(meeting.Scenario);
                foreach (IMeetingPerson person in meeting.MeetingPersons)
                {
					LazyLoadingManager.Initialize(person);
					LazyLoadingManager.Initialize(person.Person);
                    foreach (IPersonPeriod personPeriod in person.Person.PersonPeriodCollection)
                    {
                        LazyLoadingManager.Initialize(personPeriod);
						LazyLoadingManager.Initialize(personPeriod.Team);
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
