using System.Data;
using Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// Holds info of mapped objectpaircollections
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/23/2007
    /// </remarks>
    public class MappedObjectPair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappedObjectPair"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-24
        /// </remarks>
        public MappedObjectPair()
        {
            AbsenceActivity = new ObjectPairCollection<Absence, Activity>();
            OvertimeUnderlyingActivity = new ObjectPairCollection<Activity, Activity>();
            OvertimeActivity = new ObjectPairCollection<Activity,Overtime>();
        }

        /// <summary>
        /// Gets or sets the overtime activity.
        /// </summary>
        /// <value>The overtime activity.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-24
        /// </remarks>
        public ObjectPairCollection<Activity, Overtime> OvertimeActivity { get; private set; }

        /// <summary>
        /// Gets or sets the overtime underlying activity.
        /// Do not use this one to resolve an activity. Use ResolveActivity instead as it handles underlying activities for previous Overtime Activities.
        /// </summary>
        /// <value>The overtime underlying activity.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-24
        /// </remarks>
        public ObjectPairCollection<Activity, Activity> OvertimeUnderlyingActivity { get; private set; }

        /// <summary>
        /// Resolves the activity.
        /// </summary>
        /// <param name="oldActivity">The old activity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-25
        /// </remarks>
        public IActivity ResolveActivity(Activity oldActivity)
        {
            var newActivity = Activity.GetPaired(oldActivity);
            if (newActivity != null) return newActivity;

            var underlyingActivity = OvertimeUnderlyingActivity.GetPaired(oldActivity);
            return Activity.GetPaired(underlyingActivity);
        }

        /// <summary>
        /// Gets the ObjectPairCollection of activity.
        /// Do not use this one to resolve an activity. Use ResolveActivity instead as it handles underlying activities for previous Overtime Activities.
        /// </summary>
        /// <value>The activity.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<Activity, IActivity> Activity { get; set; }

        /// <summary>
        /// Gets the ObjectPairCollection of absence.
        /// </summary>
        /// <value>The absence.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<Absence, IAbsence> Absence { get; set; }

        /// <summary>
        /// Gets the ObjectPairCollection of scenario.
        /// </summary>
        /// <value>The scenario.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<global::Domain.Scenario, IScenario> Scenario { get; set; }

        /// <summary>
        /// Gets the ObjectPairCollection of contract.
        /// </summary>
        /// <value>The contract.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<WorktimeType, IContract> Contract { get; set; }

        /// <summary>
        /// Gets the ObjectPairCollection of contract schedule.
        /// </summary>
        /// <value>The contract schedule.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<WorktimeType, IContractSchedule> ContractSchedule { get; set; }

        /// <summary>
        /// Gets the ObjectPairCollection of part time percentage.
        /// </summary>
        /// <value>The part time percentage.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<WorktimeType, IPartTimePercentage> PartTimePercentage { get; set; }

        /// <summary>
        /// Gets the ObjectPairCollection of shift category.
        /// </summary>
        /// <value>The shift category.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<ShiftCategory, IShiftCategory> ShiftCategory { get; set; }

        /// <summary>
        /// Gets the ObjectPairCollection of workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<Forecast, IWorkload> Workload { get; set; }

        /// <summary>
        /// Gets or sets the ObjectPairCollection of team.
        /// </summary>
        /// <value>The team.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public ObjectPairCollection<UnitSub, ITeam> Team { get; set; }

        /// <summary>
        /// Gets or sets the type of the skill.
        /// </summary>
        /// <value>The type of the skill.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public ObjectPairCollection<SkillType, ISkillType> SkillType { get; set; }

        /// <summary>
        /// Gets or sets the queue source.
        /// </summary>
        /// <value>The queue source.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public ObjectPairCollection<int, IQueueSource> QueueSource { get; set; }

        /// <summary>
        /// Gets or sets the ACD logins.
        /// </summary>
        /// <value>The ACD logins.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 6/18/2008
        /// </remarks>
        public ObjectPairCollection<int, IExternalLogOn> ExternalLogOn { get; set; }

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        /// <value>The site.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-29
        /// </remarks>
        public ObjectPairCollection<Unit, ISite> Site { get; set; }

        /// <summary>
        /// Gets or sets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public ObjectPairCollection<Skill, ISkill> Skill { get; set; }

        /// <summary>
        /// Gets or sets the persons who are "agents".
        /// </summary>
        /// <value>The agent.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-29
        /// </remarks>
        public ObjectPairCollection<Agent, IPerson> Agent { get; set; }

        /// <summary>
        /// Gets or sets the work shift rule set.
        /// </summary>
        /// <value>The work shift rule set.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-20
        /// </remarks>
        public ObjectPairCollection<UnitEmploymentType, IRuleSetBag> RuleSetBag { get; set; }

        /// <summary>
        /// Gets or sets the grouping.
        /// </summary>
        /// <value>The grouping.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        public ObjectPairCollection<Grouping, IGroupPage> Grouping { get; set; }

        /// <summary>
        /// Gets or sets the optionalcolumn.
        /// </summary>
        /// <value>The optionalcolumn.</value>
        /// <remarks>
        /// Created by: Pubudu Kaskara
        /// Created date: 08/08/2008
        /// </remarks>
        public ObjectPairCollection<EmployeeOptionalColumn, IOptionalColumn> OptionalColumn { get; set; }

        /// <summary>
        /// Gets or sets the absence activity.
        /// </summary>
        /// <value>The absence activity.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-07-18
        /// </remarks>
        public ObjectPairCollection<Absence, Activity> AbsenceActivity { get; set; }

        /// <summary>
        /// Gets or sets the day off planner rules.
        /// </summary>
        /// <value>The day off planner rules.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-02    
        /// /// </remarks>
        public ObjectPairCollection<FreeDayPlannerSettings, DayOffPlannerRules> DayOffPlannerRules { get; set; }

        /// <summary>
        /// Gets or sets the rotations.
        /// </summary>
        /// <value>The rotations.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-25    
        /// /// </remarks>
        public ObjectPairCollection<DataRow, IRotation> Rotations { get; set; }

        /// <summary>
        /// Gets or sets the availability.
        /// </summary>
        /// <value>The availability.</value>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public ObjectPairCollection<DataRow, IAvailabilityRotation> Availability { get; set; }

        /// <summary>
        /// Gets or sets the person availability.
        /// </summary>
        /// <value>The person availability.</value>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public ObjectPairCollection<DataRow, IPersonAvailability> PersonAvailability { get; set; }

        /// <summary>
        /// Gets or sets the person rotations.
        /// </summary>
        /// <value>The person rotations.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-25    
        /// /// </remarks>
        public ObjectPairCollection<DataRow, IPersonRotation> PersonRotations { get; set; }

        /// <summary>
        /// Gets or sets the day off.
        /// </summary>
        /// <value>The day off.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        public ObjectPairCollection<Absence, IDayOffTemplate> DayOff { get; set; }

        /// <summary>
        /// Gets or sets the multiplicator definition set.
        /// </summary>
        /// <value>The multiplicator definition set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-24
        /// </remarks>
        public ObjectPairCollection<Overtime, IMultiplicatorDefinitionSet> MultiplicatorDefinitionSet { get; set; }

        /// <summary>
        /// Gets or sets the student availability.
        /// </summary>
        /// <value>The student availability.</value>
        /// <remarks>
        /// Created by: Jonas n
        /// Created date: 2011-02-08
        /// </remarks>
        public ObjectPairCollection<global::Domain.AgentDay, IStudentAvailabilityDay> StudentAvailabilityDay { get; set; }

        /// <summary>
        /// Gets or sets the Overtime availability.
        /// </summary>
        /// <value>The overtime availability.</value>
        /// <remarks>
        /// Created by: Asad m
        /// </remarks>
        public ObjectPairCollection<global::Domain.AgentDay, IOvertimeAvailability> OvertimeAvailability { get; set; }
    }
}
