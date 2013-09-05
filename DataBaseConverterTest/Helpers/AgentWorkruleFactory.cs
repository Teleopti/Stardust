using System;
using Domain;
using Infrastructure;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using TimePeriod=Domain.TimePeriod;

namespace Teleopti.Ccc.DatabaseConverterTest.Helpers
{
    /// <summary>
    /// Helpers
    /// </summary>
    public class AgentWorkRuleFactory
    {
        private ObjectPairCollection<Agent, Domain.Common.Person> _agentPairList =
            new ObjectPairCollection<Agent, Domain.Common.Person>();

        private ObjectPairCollection<WorktimeType, Contract> _contractMapList =
            new ObjectPairCollection<WorktimeType, Contract>();

        private Contract _contractFixed = new Contract("Fixed Staff");
        private Contract _contractHourly = new Contract("Hourly Staff");

        private ObjectPairCollection<WorktimeType, ContractSchedule> _contractScheduleMapList =
            new ObjectPairCollection<WorktimeType, ContractSchedule>();

        private ContractSchedule _contractScheduleFixed = new ContractSchedule("Default Fixed");
        private ContractSchedule _contractScheduleHourly = new ContractSchedule("Default Hourly");

        private ObjectPairCollection<WorktimeType, PartTimePercentage> _partTimePercentageMapList =
            new ObjectPairCollection<WorktimeType, PartTimePercentage>();

        private PartTimePercentage _partTimePercentage = new PartTimePercentage("Default");

        private Agent _oldAgent =
            new Agent(-1, "Kalle", "Kula", "Kalle@Kula.nu", "", new CccListCollection<AgentWorkrule>(),
                      new CccListCollection<AgentPeriod>(), null, "Test note");

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentWorkRuleFactory"/> class.
        /// </summary>
        public AgentWorkRuleFactory()
        {
            _contractFixed.EmploymentType = Interfaces.Domain.EmploymentType.FixedStaffNormalWorkTime;
            _contractFixed.WorkTime = new WorkTime(new TimeSpan(8, 0, 0));

            _contractHourly.EmploymentType = Interfaces.Domain.EmploymentType.HourlyStaff;
            _contractHourly.WorkTime = new WorkTime(new TimeSpan(0, 0, 0));

            _contractMapList.Add((WorktimeType) 1, _contractFixed);
            _contractMapList.Add((WorktimeType) 2, _contractFixed);
            _contractMapList.Add((WorktimeType) 3, _contractFixed);
            _contractMapList.Add((WorktimeType) 4, _contractHourly);
            _contractMapList.Add((WorktimeType) 5, _contractHourly);

            _contractScheduleMapList.Add((WorktimeType) 1, _contractScheduleFixed);
            _contractScheduleMapList.Add((WorktimeType) 2, _contractScheduleFixed);
            _contractScheduleMapList.Add((WorktimeType) 3, _contractScheduleFixed);
            _contractScheduleMapList.Add((WorktimeType) 4, _contractScheduleHourly);
            _contractScheduleMapList.Add((WorktimeType) 5, _contractScheduleHourly);

            _partTimePercentageMapList.Add((WorktimeType) 1, _partTimePercentage);
            _partTimePercentageMapList.Add((WorktimeType) 2, _partTimePercentage);
            _partTimePercentageMapList.Add((WorktimeType) 3, _partTimePercentage);
            _partTimePercentageMapList.Add((WorktimeType) 4, _partTimePercentage);
            _partTimePercentageMapList.Add((WorktimeType) 5, _partTimePercentage);

            Domain.Common.Person newAgent = new Domain.Common.Person();
            _agentPairList.Add(_oldAgent, newAgent);
        }

        /// <summary>
        /// Gets the agent pair list.
        /// </summary>
        /// <value>The agent pair list.</value>
        public ObjectPairCollection<Agent, Domain.Common.Person> AgentPairList
        {
            get { return _agentPairList; }
        }

        /// <summary>
        /// Gets the contract pair list.
        /// </summary>
        /// <value>The contract pair list.</value>
        public ObjectPairCollection<WorktimeType, Contract> ContractPairList
        {
            get { return _contractMapList; }
        }

        /// <summary>
        /// Gets the contract schedule pair list.
        /// </summary>
        /// <value>The contract schedule pair list.</value>
        public ObjectPairCollection<WorktimeType, ContractSchedule> ContractSchedulePairList
        {
            get { return _contractScheduleMapList; }
        }

        /// <summary>
        /// Gets the part time percentage pair list.
        /// </summary>
        /// <value>The part time percentage pair list.</value>
        public ObjectPairCollection<WorktimeType, PartTimePercentage> PartTimePercentagePairList
        {
            get { return _partTimePercentageMapList; }
        }

        /// <summary>
        /// Gets the contract pair list.
        /// </summary>
        /// <value>The contract pair list.</value>
        public ObjectPairCollection<WorktimeType, Contract> ContractMapList
        {
            get { return _contractMapList; }
        }

        /// <summary>
        /// Get the old agent
        /// </summary>
        public Agent OldAgent
        {
            get { return _oldAgent; }
        }

        /// <summary>
        /// CreateProjection a new AgentPeriod with the date period information given
        /// </summary>
        /// <param name="datePeriod">Date period to use.</param>
        /// <returns></returns>
        public static AgentPeriod AgentPeriod(DatePeriod datePeriod)
        {
            AgentPeriod agentPeriod =
                new AgentPeriod(datePeriod, null,
                                new global::Domain.EmploymentType(-1, "testType", 0, new TimeSpan(11, 0, 0),
                                                                  new TimeSpan(36, 0, 0),
                                                                  new TimeSpan(1, 0, 0)), null, null, null, null,
                                String.Empty, null,
                                new CccListCollection<AgentSkill>(), DateTime.Today);

            return agentPeriod;
        }

        /// <summary>
        /// CreateProjection a new work rule with the specified properties
        /// </summary>
        /// <param name="datePeriod">Date period to use.</param>
        /// <param name="typeOfWorkTime">Type of work time.</param>
        /// <returns>AgentWorkrule</returns>
        public static AgentWorkrule AgentWorkRule(DatePeriod datePeriod, WorktimeType typeOfWorkTime)
        {
            AgentWorkrule agentWorkrule =
                new AgentWorkrule(datePeriod, new TimePeriod(new TimeSpan(40, 0, 0)), new TimeSpan(),
                                  new TimeSpan(), 0, 0, new TimeSpan(8, 0, 0), typeOfWorkTime, new DatePeriod(),
                                  new CccListCollection<ShiftCategoryLimitation>(), 1, 4);

            return agentWorkrule;
        }
    }
}