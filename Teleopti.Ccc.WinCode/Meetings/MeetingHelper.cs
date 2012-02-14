using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings
{
    public class MeetingHelper : IMeetingHelper
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IMeetingRepository _meetingRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IScenarioRepository _scenarioRepository;

        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-05
        /// </remarks>
        public IUnitOfWork UnitOfWork { get; set;}

        public MeetingHelper(IUnitOfWork uow)
        {
            UnitOfWork = uow;
            _repositoryFactory = new RepositoryFactory();
            _meetingRepository = _repositoryFactory.CreateMeetingRepository(UnitOfWork);
            _personRepository = _repositoryFactory.CreatePersonRepository(UnitOfWork);
            _activityRepository = _repositoryFactory.CreateActivityRepository(UnitOfWork);
            _scenarioRepository = _repositoryFactory.CreateScenarioRepository(UnitOfWork);
        }

        /// <summary>
        /// Adds the meeting.
        /// </summary>
        /// <param name="meeting">The meeting.</param>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-05
        /// </remarks>
        public void SaveMeeting(IMeeting meeting)
        {
            _meetingRepository.Add(meeting);
        }

        /// <summary>
        /// Removes the meeting.
        /// </summary>
        /// <param name="meeting">The meeting.</param>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 2008-12-05
        /// </remarks>
        public void DeleteMeeting(IMeeting meeting)
        {
            _meetingRepository.Remove(meeting);
        }
        
        /// <summary>
        /// Gets the persons.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 12/23/2008
        /// </remarks>
        public IList<IPerson> GetPersons(DateTimePeriod period)
        {
            return _personRepository.FindPeopleInOrganization(period).ToList();
        }

        /// <summary>
        /// Gets the activities.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 12/23/2008
        /// </remarks>
        public IList<IActivity> GetActivities()
        {
            return _activityRepository.LoadAllSortByName();
        }

        /// <summary>
        /// Gets the scenarios.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 12/23/2008
        /// </remarks>
        public IList<IScenario> GetScenarios()
        {
            return _scenarioRepository.FindAllSorted();
        }

        /// <summary>
        /// Gets all meetings.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 12/23/2008
        /// </remarks>
        public IList<IMeeting> GetAllMeetings(IList<IPerson> persons, DateTimePeriod dateTimePeriod, IScenario scenario)
        {
            return (IList<IMeeting>)(_meetingRepository).Find(persons, dateTimePeriod, scenario);
        }
    }
}