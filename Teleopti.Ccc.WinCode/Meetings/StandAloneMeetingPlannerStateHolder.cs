#region Imports

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using System.Linq;

#endregion

namespace Teleopti.Ccc.WinCode.Meetings
{
    /// <summary>
    /// Implements AbstractMeetingPlannerStateHolder. 
    /// This state-holder is supposed to retrieve the required data from the persistant storage and pass them 
    /// to the Meeting Organizer/Composer. 
    /// </summary>
    public class StandAloneMeetingPlannerStateHolder : AbstractMeetingPlannerStateHolder
    {
        #region Fields - Instance Member

        /// <summary>
        /// Holds an implentation of <see cref="IScheduleRepository"/>
        /// </summary>
        private IScheduleRepository _scheduleRepository;

        /// <summary>
        /// Holds an implentation of <see cref="IScenarioRepository"/>
        /// </summary>
        private IScenarioRepository _scenarioRepository;

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - StandAloneMeetingPlannerStateHolder Members - Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StandAloneMeetingPlannerStateHolder"/> class and 
        /// assingns the passed in repositories to the private variables. 
        /// </summary>
        /// <param name="scheduleRepository">An implementation of <see cref="IScheduleRepository"/></param>
        /// <param name="scenarioRepository">An implementation of <see cref="IScenarioRepository"/></param>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public StandAloneMeetingPlannerStateHolder(IScheduleRepository scheduleRepository,
                                                   IScenarioRepository scenarioRepository)
        {
            _scheduleRepository = scheduleRepository;
            _scenarioRepository = scenarioRepository;
        }

        #endregion

        #region Methods - Instance Member - StandAloneMeetingPlannerStateHolder Members

        /// <summary>
        /// Sets all persons.
        /// </summary>
        /// <param name="personCollection">The person collection.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-15
        /// </remarks>
        public void SetAllPersons(ICollection<IPerson> personCollection)
        {
            base.AllPersonCollection = personCollection.ToList();
        }

        #endregion

        #region Methods - Instance Member - StandAloneMeetingPlannerStateHolder Members - Base Class Members

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
        public override void LoadSchedules(IList<IPerson> persons, DateTimePeriod period, IScenario scenario, ISystemSettingReader systemSettingReader)
        {
            base.ScheduleDictionary = _scheduleRepository.Find(persons, period, scenario, systemSettingReader);
        }

        /// <summary>
        /// Loads the relevant <see cref="IScenario"/> collection.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 23/09/2008
        /// </remarks>
        public override void LoadAllScenarios()
        {
            base.ScenarioCollection = _scenarioRepository.FindAllSorted();
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
    }
}