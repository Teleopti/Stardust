using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.Helpers
{
    /// <summary>
    /// Factory helper for skill day tests
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-31
    /// </remarks>
    public class SkillDayFactory
    {
        ObjectPairCollection<global::Domain.Scenario, IScenario> _scenarioPair = 
            new ObjectPairCollection<global::Domain.Scenario, IScenario>();

        ObjectPairCollection<global::Domain.Forecast, IWorkload> _workloadPair = 
            new ObjectPairCollection<global::Domain.Forecast, IWorkload>();

        ObjectPairCollection<global::Domain.Skill, ISkill> _skillPair = 
            new ObjectPairCollection<global::Domain.Skill, ISkill>();
            
        global::Domain.Scenario _oldScenario = new global::Domain.Scenario(-1, "Non-Default", false, false);
        IScenario _newScenario = new Scenario("Non-Default");

        global::Domain.Activity _oldActivity = new global::Domain.Activity(-1, "", System.Drawing.Color.DodgerBlue, false, false, false, false, false, false, false, false);

        ISkill _newSkill = new Skill("Skill", "Descr.", Color.DodgerBlue, 15, SkillTypeFactory.CreateSkillType());
        global::Domain.Skill _oldSkill;

        global::Domain.Forecast _oldForecast = new global::Domain.Forecast(12, "Name", "Desc");
        IWorkload _newWorkload;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDayFactory"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public SkillDayFactory()
        {
            _scenarioPair.Add(_oldScenario, _newScenario);

            _newWorkload = new Workload(_newSkill);
            _workloadPair.Add(_oldForecast, _newWorkload);

            _oldSkill = new global::Domain.Skill(-1,
                global::Domain.SkillType.Dummy,
                "Skill", "Skill", Color.DodgerBlue,
                _oldActivity, _oldActivity,
                new global::Infrastructure.CccListCollection<global::Domain.Forecast> { _oldForecast },
                false, true, new global::Domain.AgentRequestLimit(10, 11, 12), null, 3);
            _skillPair.Add(_oldSkill, _newSkill);
        }

        /// <summary>
        /// Gets the old skill.
        /// </summary>
        /// <value>The old skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public global::Domain.Skill OldSkill
        {
            get { return _oldSkill; }
        }

        /// <summary>
        /// Gets the old forecast.
        /// </summary>
        /// <value>The old forecast.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public global::Domain.Forecast OldForecast
        {
            get { return _oldForecast; }
        }

        /// <summary>
        /// Gets the old scenario.
        /// </summary>
        /// <value>The old scenario.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public global::Domain.Scenario OldScenario
        {
            get { return _oldScenario; }
        }

        /// <summary>
        /// Gets the new scenario.
        /// </summary>
        /// <value>The new scenario.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public IScenario NewScenario
        {
            get { return _newScenario; }
        }

        /// <summary>
        /// Gets the new skill.
        /// </summary>
        /// <value>The new skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public ISkill NewSkill
        {
            get { return _newSkill; }
        }

        /// <summary>
        /// Gets the new workload.
        /// </summary>
        /// <value>The new workload.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public IWorkload NewWorkload
        {
            get { return _newWorkload; }
        }

        /// <summary>
        /// Gets the scenario pair.
        /// </summary>
        /// <value>The scenario pair.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public ObjectPairCollection<global::Domain.Scenario, IScenario> ScenarioPair
        {
            get { return _scenarioPair; }
        }

        /// <summary>
        /// Gets the workload pair.
        /// </summary>
        /// <value>The workload pair.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public ObjectPairCollection<global::Domain.Forecast, IWorkload> WorkloadPair
        {
            get { return _workloadPair; }
        }

        /// <summary>
        /// Gets the skill pair.
        /// </summary>
        /// <value>The skill pair.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-31
        /// </remarks>
        public ObjectPairCollection<global::Domain.Skill, ISkill> SkillPair
        {
            get { return _skillPair; }
        }
    }
}
