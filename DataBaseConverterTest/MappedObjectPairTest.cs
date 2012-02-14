using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    /// <summary>
    /// Tests for MappedObjectPair
    /// </summary>
    [TestFixture]
    public class MappedObjectPairTest
    {
        private MappedObjectPair target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new MappedObjectPair();
        }

        /// <summary>
        /// Verifies the default values.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        [Test]
        public void VerifyDefaultValues()
        {
            Assert.IsNull(target.Activity);
            Assert.IsNull(target.Absence);
            Assert.IsNull(target.Scenario);
            Assert.IsNull(target.Agent);
            Assert.IsNull(target.Contract);
            Assert.IsNull(target.ContractSchedule);
            Assert.IsNull(target.PartTimePercentage);
            Assert.IsNull(target.ShiftCategory);
            Assert.IsNull(target.Workload);
            Assert.IsNull(target.Team);
            Assert.IsNull(target.SkillType);
            Assert.IsNull(target.QueueSource);
            Assert.IsNull(target.ExternalLogOn);
        }

        /// <summary>
        /// Determines whether this instance [can set properties].
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        [Test]
        public void CanSetProperties()
        {
            ObjectPairCollection<global::Domain.Activity, IActivity> mappedActivities = new ObjectPairCollection<global::Domain.Activity, IActivity>();
            ObjectPairCollection<global::Domain.Absence, IAbsence> mappedAbsences = new ObjectPairCollection<global::Domain.Absence, IAbsence>();
            ObjectPairCollection<global::Domain.Scenario, IScenario> mappedScenarios = new ObjectPairCollection<global::Domain.Scenario, IScenario>();
            ObjectPairCollection<global::Domain.Agent, IPerson> mappedAgents = new ObjectPairCollection<global::Domain.Agent, IPerson>();
            ObjectPairCollection<global::Domain.WorktimeType, IContract> mappedContracts = new ObjectPairCollection<global::Domain.WorktimeType, IContract>();
            ObjectPairCollection<global::Domain.WorktimeType, IContractSchedule> mappedContractSchedules = new ObjectPairCollection<global::Domain.WorktimeType, IContractSchedule>();
            ObjectPairCollection<global::Domain.WorktimeType, IPartTimePercentage> mappedPartTimePercentages = new ObjectPairCollection<global::Domain.WorktimeType, IPartTimePercentage>();
            ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory> mappedShiftCategories = new ObjectPairCollection<global::Domain.ShiftCategory, IShiftCategory>();
            ObjectPairCollection<global::Domain.Forecast, IWorkload> mappedWorkloads = new ObjectPairCollection<global::Domain.Forecast, IWorkload>();
            ObjectPairCollection<global::Domain.UnitSub, ITeam> mappedTeams = new ObjectPairCollection<global::Domain.UnitSub, ITeam>();
            ObjectPairCollection<global::Domain.SkillType, ISkillType> mappedSkilltypes = new ObjectPairCollection<global::Domain.SkillType, ISkillType>();
            ObjectPairCollection<int, IQueueSource> mappedQueueSources = new ObjectPairCollection<int, IQueueSource>();
            ObjectPairCollection<int, IExternalLogOn> mappedLogins = new ObjectPairCollection<int, IExternalLogOn>();

            target.Activity = mappedActivities;
            target.Absence = mappedAbsences;
            target.Scenario = mappedScenarios;
            target.Agent = mappedAgents;
            target.Contract = mappedContracts;
            target.ContractSchedule = mappedContractSchedules;
            target.PartTimePercentage = mappedPartTimePercentages;
            target.ShiftCategory = mappedShiftCategories;
            target.Workload = mappedWorkloads;
            target.Team = mappedTeams;
            target.SkillType = mappedSkilltypes;
            target.QueueSource = mappedQueueSources;
            target.ExternalLogOn = mappedLogins;

            Assert.AreSame(mappedActivities, target.Activity);
            Assert.AreSame(mappedAbsences, target.Absence);
            Assert.AreSame(mappedScenarios, target.Scenario);
            Assert.AreSame(mappedAgents, target.Agent);
            Assert.AreSame(mappedContracts, target.Contract);
            Assert.AreSame(mappedContractSchedules, target.ContractSchedule);
            Assert.AreSame(mappedPartTimePercentages, target.PartTimePercentage);
            Assert.AreSame(mappedShiftCategories, target.ShiftCategory);
            Assert.AreSame(mappedWorkloads, target.Workload);
            Assert.AreSame(mappedTeams, target.Team);
            Assert.AreSame(mappedSkilltypes, target.SkillType);
            Assert.AreSame(mappedQueueSources, target.QueueSource);
            Assert.AreSame(mappedLogins, target.ExternalLogOn);
        }
    }

 
}
