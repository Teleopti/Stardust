using System;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Common;
using EmploymentType = Domain.EmploymentType;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class AgentContractMapperTest : MapperTest<Agent>
    {
        private AgentWorkRuleFactory agWorkruleFactory;
        private Agent oldAgent;
        private Domain.Common.Person newAgent;
        private AgentContractMapper target;
        private MappedObjectPair mappedObjectPair;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            agWorkruleFactory = new AgentWorkRuleFactory();
            oldAgent = agWorkruleFactory.OldAgent;
            mappedObjectPair = new MappedObjectPair();

            newAgent = new Domain.Common.Person();
            ObjectPairCollection<Agent, Domain.Common.Person> paircoll =
                new ObjectPairCollection<Agent, Domain.Common.Person>();
            paircoll.Add(oldAgent, newAgent);
            mappedObjectPair.Agent = paircoll;
            target = new AgentContractMapper(mappedObjectPair, TimeZoneInfo.Local, DateTime.Today);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 9; }
        }

        [Test]
        public void VerifyNullIsReturnedIfPeriodDoNotExist()
        {
            Assert.IsNull(target.Map(oldAgent));
        }

        [Test]
        public void VerifyNullIsReturnedIfOnlyNonValidPeriodExists()
        {
            AgentWorkrule workRule =
                new AgentWorkrule(new DatePeriod(new DateTime(1900, 1, 1), DateTime.Today.AddDays(-1)),
                                  new TimePeriod(StandardTimeLimit.WholeDay),
                                  new TimeSpan(234),
                                  new TimeSpan(123),
                                  3, 3, new TimeSpan(1333),
                                  WorktimeType.Blajj,
                                  new DatePeriod(new DateTime(1907, 1, 1), new DateTime(1908, 3, 3)),
                                  new CccListCollection<ShiftCategoryLimitation>());
            oldAgent.WorkruleCollection.Add(workRule);
            Assert.IsNull(target.Map(oldAgent));
        }

        [Test]
        public void VerifyMappingWorks()
        {
            AgentWorkrule workRule =
                        new AgentWorkrule(new DatePeriod(new DateTime(1900, 1, 1), DateTime.Today.AddDays(1)),
                      new TimePeriod(StandardTimeLimit.WholeDay),
                      new TimeSpan(234),
                      new TimeSpan(123),
                      3, 3, new TimeSpan(1333),
                      WorktimeType.Blajj,
                      new DatePeriod(new DateTime(1907, 1, 1), new DateTime(1908, 3, 3)),
                      new CccListCollection<ShiftCategoryLimitation>());
            oldAgent.WorkruleCollection.Add(workRule);

            EmploymentType empType = new EmploymentType(1, "", 1, new TimeSpan(1), new TimeSpan(2), new TimeSpan(4));
            AgentPeriod agentPeriod =
                new AgentPeriod(new DatePeriod(new DateTime(1900, 1, 1), DateTime.Today.AddDays(1)),
                                1, empType, null, null, null, null, "", null, null, DateTime.Now);
            oldAgent.PeriodCollection.Add(agentPeriod);

            ObjectPairCollection<WorktimeType, Contract> paircollContract =
                new ObjectPairCollection<WorktimeType, Contract>();
            Contract contract = new Contract("foo");
            paircollContract.Add(WorktimeType.Blajj, contract);
            mappedObjectPair.Contract = paircollContract;
            ObjectPairCollection<WorktimeType, PartTimePercentage> paircollPartTimePercent =
                new ObjectPairCollection<WorktimeType, PartTimePercentage>();
            PartTimePercentage partTime = new PartTimePercentage("ff");
            paircollPartTimePercent.Add(WorktimeType.Blajj, partTime);
            mappedObjectPair.PartTimePercentage = paircollPartTimePercent;
            ObjectPairCollection<WorktimeType, ContractSchedule> paircollContractSched =
                new ObjectPairCollection<WorktimeType, ContractSchedule>();
            ContractSchedule contractSched = new ContractSchedule("#ff");
            paircollContractSched.Add(WorktimeType.Blajj, contractSched);
            mappedObjectPair.ContractSchedule = paircollContractSched;

            Domain.AgentInfo.PersonContract mappedContract = target.Map(oldAgent);

            Assert.IsNotNull(mappedContract);
            Assert.AreSame(mappedContract.Person, newAgent);
            Assert.AreSame(mappedContract.Contract, contract);
        }
    }
}
