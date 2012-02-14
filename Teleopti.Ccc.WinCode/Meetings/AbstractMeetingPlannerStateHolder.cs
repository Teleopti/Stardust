#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.WinCode.Meetings
{
    /// <summary>
    /// Spcifies the methods that needs to be implemented in state-holders that are to be used
    /// in meeting organizer/composer. Currently there are two implementations.
    /// 
    /// 1. <see cref="StandAloneMeetingPlannerStateHolder"/>, which is to be used when <B>Meetings</B> is lounched 
    ///     from the ribbon bar Meetings  button. This will load the necessary information from the persistant storage. 
    /// 
    /// 2. <see cref="SchedulerMeetingPlannerStateHolder"/>, which is to be used when <B>Meetings</B> is launched 
    ///     from within the Scheduler subsystem. This will load the information from the passed in <see cref="SchedulerStateHolder"/>.
    /// 
    /// The main intention is using the second stateholder is to increase the response time 
    /// by using the already loade data from the <see cref="SchedulerStateHolder"/>.
    /// </summary>
    public abstract class AbstractMeetingPlannerStateHolder
    {
        #region Fields - Instance Member

        /// <summary>
        /// To hold the selected date
        /// </summary>
        private DateTime _selectedDate;

        /// <summary>
        /// To hold the meeting collection
        /// </summary>
        private IList<IMeeting> _meetingList;

        /// <summary>
        /// To hold the <see cref="MeetingSchedulerGridView"/> adpter list
        /// </summary>
        private readonly IList<MeetingSchedulerGridView> _meetingSchedulerAdapterList =
            new List<MeetingSchedulerGridView>();

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - AbstractMeetingPlannerStateHolder Members

        /// <summary>
        /// Gets or sets the selected Date
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set { _selectedDate = value; }
        }

        /// <summary>
        /// Gets the Meeting Collection
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public IList<IMeeting> MeetingCollection
        {
            get { return _meetingList; }
        }

        /// <summary>
        /// Gets the meeting scheduler adapter list.
        /// </summary>
        /// <value>The meeting scheduler adapter list.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-08
        /// </remarks>
        public IList<MeetingSchedulerGridView> MeetingSchedulerAdapterList
        {
            get { return _meetingSchedulerAdapterList; }
        }

        /// <summary>
        /// The collection of Meeting Persons that holds all persons
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public IList<IPerson> AllPersonCollection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="IPerson"/>s those who are supposed to appear in the meeting composer
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public IList<IPerson> SelectedPersonCollection
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the Schedule collection
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public IScheduleDictionary ScheduleDictionary
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IScenario"/> collection
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public IList<IScenario> ScenarioCollection
        {
            get;
            protected set;
        }

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - AbstractMeetingPlannerStateHolder Members

        #region Methods - Instance Member - AbstractMeetingPlannerStateHolder Members - Abstract Methods

        /// <summary>
        /// Loads schedules for the passed in persons based on the passed in period (<seealso cref="DateTimePeriod"/>) and
        /// the scenario (<seealso cref="IScenario"/>).
        /// </summary>
        /// <param name="persons">The <see cref="IPerson"/> collection</param>
        /// <param name="period">The selected <see cref="DateTimePeriod"/></param>
        /// <param name="scenario">The selected <see cref="IScenario"/></param>
        /// <param name="systemSettingReader">The system setting reader.</param>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public abstract void LoadSchedules(IList<IPerson> persons, DateTimePeriod period, IScenario scenario, ISystemSettingReader systemSettingReader);

        /// <summary>
        /// Loads the relevant <see cref="IScenario"/> collection.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public abstract void LoadAllScenarios();

        /// <summary>
        /// Sets a collection of <see cref="IPerson"/>.
        /// </summary>
        /// <param name="selectedPersonCollection">The <see cref="IPerson"/> list to be set</param>
        public abstract void SetSelectedMeetingPersonCollection(IList<IPerson> selectedPersonCollection);

        #endregion

        #region Methods - Instance Member - AbstractMeetingPlannerStateHolder Members

        /// <summary>
        /// Removes the passed in meeting from the repository.
        /// </summary>
        /// <param name="meeting">The meeting to be removed</param>
        /// <param name="meetingRepository">An implementation of <see cref="IMeetingRepository"/></param>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public void RemoveMeeting(IRepository<IMeeting> meetingRepository, IMeeting meeting)
        {
            meetingRepository.Remove(meeting);
        }

        /// <summary>
        /// Adds a meeting to the repository persistant storage.
        /// </summary>
        /// <param name="meeting">Teh meeting to be added</param>
        /// <param name="meetingRepository">An implementation of <see cref="IMeetingRepository"/></param>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public void AddMeeting(IRepository<IMeeting> meetingRepository, IMeeting meeting)
        {
            meetingRepository.Add(meeting);
        }

        /// <summary>
        /// Loads all meetings from the persistant storage.
        /// </summary>
        /// </summary>
        /// <param name="persons"></param>
        /// <param name="dateTimePeriod"></param>
        /// <param name="scenario"></param>
        /// <param name="meetingRepository"></param>
        /// /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public void LoadMeetings(IList<IPerson> persons, DateTimePeriod dateTimePeriod, IScenario scenario,
                                 IMeetingRepository meetingRepository)
        {
            _meetingList = (IList<IMeeting>)meetingRepository.Find(persons, dateTimePeriod, scenario);
        }

        #endregion

        #region Methods - Instance Member - AbstractMeetingPlannerStateHolder Members - Private Methods

        /// <summary>
        /// Creates the meeting scheduler adapter list.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-08
        /// </remarks>
        public void CreateMeetingSchedulerAdapterList()
        {
            _meetingSchedulerAdapterList.Clear();
            foreach (KeyValuePair<IPerson, IScheduleRange> scheduleRane in ScheduleDictionary)
            {
                MeetingSchedulerGridView meetingSchedulerGridView = new MeetingSchedulerGridView(scheduleRane.Value);
                _meetingSchedulerAdapterList.Add(meetingSchedulerGridView);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}