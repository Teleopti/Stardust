#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.WinCode.Meetings
{
    /// <summary>
    /// Implements AbstractMeetingPlannerStateHolder. 
    /// This state-holder is supposed to extract data from the passed in <see cref="SchedulerStateHolder"/> and feed 
    /// them to the Meeting Organizer/Composer. 
    /// </summary>
    public class SchedulerMeetingPlannerStateHolder : AbstractMeetingPlannerStateHolder
    {
        #region Fields - Instance Member

        /// <summary>
        /// To hold the <see cref="SchedulerStateHolder"/> 
        /// which is the source of data for the <see cref="SchedulerMeetingPlannerStateHolder"/>
        /// </summary>
        private readonly SchedulerStateHolder _schedulerStateHolder;

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - SchedulerMeetingPlannerStateHolder Members

        #region Methods - Instance Member - SchedulerMeetingPlannerStateHolder Members - Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerMeetingPlannerStateHolder"/> class.
        /// </summary>
        /// <param name="schedulerStateHolder">The scheduler state holder.</param>
        /// <param name="selectedPersonCollection">The selected person collection.</param>
        public SchedulerMeetingPlannerStateHolder(SchedulerStateHolder schedulerStateHolder,
                                                  IList<IPerson> selectedPersonCollection)
        {
            _schedulerStateHolder = schedulerStateHolder;
            base.SelectedPersonCollection = selectedPersonCollection;

            base.AllPersonCollection = _schedulerStateHolder.AllPermittedPersons;
        }

        #endregion

        #region Methods - Instance Member - SchedulerMeetingPlannerStateHolder Members - Base Class Members

        /// <summary>
        /// Loads schedules based on the passed in date
        /// </summary>
        /// <param name="persons">The enumerable Person collection who's schedules are to be loaded</param>
        /// <param name="period">The selected Period based on which the schedules are to be loaded</param>
        /// <param name="scenario">The selected <see cref="IScenario"/></param>
        /// <param name="systemSettingReader">The system setting reader.</param>
        /// <remarks>
        /// Created by:shirang
        /// Created date: 2008-10-01
        /// </remarks>
        public override void LoadSchedules(IList<IPerson> persons, DateTimePeriod period, IScenario scenario, ISystemSettingReader systemSettingReader)
        {
            if (scenario != _schedulerStateHolder.Schedules.Scenario)
                throw new ArgumentOutOfRangeException("scenario", "Scenario must be the same as loaded in Scheduler");
            if (!_schedulerStateHolder.RequestedPeriod.Contains(period))
                throw new ArgumentOutOfRangeException("period",
                                                      "Period must be contained by the period loaded in Scheduler");

            IScheduleDictionary retDic = (IScheduleDictionary) _schedulerStateHolder.Schedules.Clone();
            foreach (IPerson person in persons)
            {
                if (!retDic.ContainsKey(person))
                    throw new ArgumentOutOfRangeException("persons", "Person must be loaded in Scheduler");
            }
            IList<IPerson> personList = new List<IPerson>(persons);
            foreach (IPerson person in _schedulerStateHolder.Schedules.Keys)
            {
                if (!personList.Contains(person))
                    retDic.Remove(person);
            }
            base.ScheduleDictionary = retDic;
        }

        /// <summary>
        /// Loads the scenario collection by using the passed in Scheduler state-holder (see the constructor)
        /// </summary>
        /// <remarks>
        /// Created by:shirang
        /// Created date: 2008-10-01
        /// </remarks>
        public override void LoadAllScenarios()
        {
            base.ScenarioCollection = new List<IScenario>();
            base.ScenarioCollection.Add(_schedulerStateHolder.Schedules.Scenario);
        }

        /// <summary>
        /// Sets the selected meeting person collection.
        /// </summary>
        /// <param name="selectedPersonCollection">The selected person collection.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-09
        /// </remarks>
        public override void SetSelectedMeetingPersonCollection(IList<IPerson> selectedPersonCollection)
        {
            base.SelectedPersonCollection = selectedPersonCollection;
        }

        #endregion

        #endregion

        #endregion
    }
}