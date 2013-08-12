using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using ShiftCategoryLimitation = Domain.ShiftCategoryLimitation;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for AgentMapper
    /// </summary>
    [TestFixture]
    public class AgentMapperTest : MapperTest<global::Domain.Agent>
    {
        private AgentMapper _simpleAgentMapper;
        private global::Domain.Agent _oldAgent;
        private global::Domain.Agent _oldAgentNoPeriods;
        private global::Domain.Agent _oldAgentWithTerminalDate;
        private global::Domain.Agent _oldAgentWithNoTerminalDate;
        private Domain.Common.Person _newAgent;
        private MappedObjectPair mappedObjectPair;
        private ExternalLogOn login;
        private string _note = "Note Karthahe";
        private ShiftCategory _oldCat1;
        private ShiftCategory _oldCat2;
        
        private Domain.Scheduling.ShiftCategory _newCat1;
        private Domain.Scheduling.ShiftCategory _newCat2;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 9; }
        }

        /// <summary>
        /// Sets the up.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            SetupObjectPairs();

            
            _simpleAgentMapper = new AgentMapper(mappedObjectPair, (TimeZoneInfo.Utc), new PersonContract(new Contract("test"),new PartTimePercentage("test"),new ContractSchedule("test")));

            // Person  Period
            global::Domain.ICccListCollection<global::Domain.AgentPeriod> agPeriods = new global::Infrastructure.CccListCollection<global::Domain.AgentPeriod>();
            global::Domain.DatePeriod datePeriod = new global::Domain.DatePeriod(new DateTime(2006, 12, 31));

            global::Domain.ICccListCollection<global::Domain.AgentPeriod> agentPeriodsWithTerminalDate = new global::Infrastructure.CccListCollection<global::Domain.AgentPeriod>();
            global::Domain.ICccListCollection<global::Domain.AgentPeriod> agentPeriodsWithNoTerminalDate = new global::Infrastructure.CccListCollection<global::Domain.AgentPeriod>();
            global::Domain.DatePeriod datePeriodWithNoTerminalDate1 = new global::Domain.DatePeriod(new DateTime(2007, 6, 1), new DateTime(2008, 12, 31));
            global::Domain.DatePeriod datePeriodWithNoTerminalDate2 = new global::Domain.DatePeriod(new DateTime(2009, 1, 1), new DateTime(2059, 12, 31));
            global::Domain.DatePeriod datePeriodWithTerminalDate1 = new global::Domain.DatePeriod(new DateTime(2008, 1, 1), new DateTime(2008, 12, 31));
            global::Domain.DatePeriod datePeriodWithTerminalDate2 = new global::Domain.DatePeriod(new DateTime(2009, 1, 1), new DateTime(2009, 12, 31));

            global::Domain.UnitSub oldTeam = new global::Domain.UnitSub(-1, "Test", -1, false, null);
            BusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            mappedObjectPair.Team.Add(oldTeam,bu.SiteCollection[0].TeamCollection[0]);


            ICccListCollection<AgentSkill> skillColl = CreateSkillCollection();
            agPeriods.Add(
                new global::Domain.AgentPeriod(datePeriod, -1, null, null, oldTeam, null, null, _note, null, skillColl,
                                new DateTime(2006, 12, 31)));
            datePeriod = new global::Domain.DatePeriod(new DateTime(2007, 02, 01));
            agPeriods.Add(
                new global::Domain.AgentPeriod(datePeriod, -1, null, null, oldTeam, null, null, "", null, skillColl,
                                new DateTime(2006, 12, 31)));
            agPeriods.FinishReadingFromDatabase(global::Domain.CollectionType.Locked);

            agentPeriodsWithNoTerminalDate.Add(
                new global::Domain.AgentPeriod(datePeriodWithNoTerminalDate1, -1, null, null, oldTeam, null, null, _note, null, skillColl,
                                new DateTime(2006, 12, 31)));
            agentPeriodsWithNoTerminalDate.Add(
                new global::Domain.AgentPeriod(datePeriodWithNoTerminalDate2, -1, null, null, oldTeam, null, null, _note, null, skillColl,
                                new DateTime(2006, 12, 31)));

            agentPeriodsWithTerminalDate.Add(
                new global::Domain.AgentPeriod(datePeriodWithTerminalDate1, -1, null, null, oldTeam, null, null, _note, null, skillColl,
                                new DateTime(2006, 12, 31)));
            agentPeriodsWithTerminalDate.Add(
                new global::Domain.AgentPeriod(datePeriodWithTerminalDate2, -1, null, null, oldTeam, null, null, _note, null, skillColl,
                                new DateTime(2006, 12, 31)));

            _oldCat1 = new ShiftCategory(-1,"Cat1","O1",Color.DimGray,true,true,1);
            _oldCat2 = new ShiftCategory(-1, "Cat2", "O1", Color.DimGray, true, true, 1);
            _newCat1 = new Domain.Scheduling.ShiftCategory("Cat1");
            _newCat2 = new Domain.Scheduling.ShiftCategory("Cat2");
            ObjectPairCollection<ShiftCategory, IShiftCategory> catPairColl =
                new ObjectPairCollection<ShiftCategory, IShiftCategory>();
            catPairColl.Add(_oldCat1, _newCat1);
            catPairColl.Add(_oldCat2, _newCat2);
            mappedObjectPair.ShiftCategory = catPairColl;
            // Person 


            _oldAgent = new global::Domain.Agent(-1, "Kalle", "Kula", "Kalle@Kula.nu", "", CreateWorkruleCollection(), agPeriods, null, "Test note");

            _oldAgent.WorkruleCollection[0].CategoryLimitationCollection.Add(new ShiftCategoryLimitation(_oldCat1,3,WeekPeriod.A));
            _oldAgent.WorkruleCollection[1].CategoryLimitationCollection.Add(new ShiftCategoryLimitation(_oldCat2, 4, WeekPeriod.B));

            _oldAgentNoPeriods = new global::Domain.Agent(-1, "Kalle", "Kula", "Kalle@Kula.nu", "", CreateWorkruleCollection(), new global::Infrastructure.CccListCollection<global::Domain.AgentPeriod>(), null, "Test note");
            _oldAgentWithNoTerminalDate = new global::Domain.Agent(-1, "Kalle", "Kula", "Kalle@Kula.nu", "", CreateWorkruleCollection(), agentPeriodsWithNoTerminalDate, null, "No terminal date");
            _oldAgentWithTerminalDate = new global::Domain.Agent(-1, "Kalle", "Kula", "Kalle@Kula.nu", "", CreateWorkruleCollection(), agentPeriodsWithTerminalDate, null, "Terminal date");
            
            // Person Contracts
            _newAgent = new Domain.Common.Person();
            ObjectPairCollection<Agent, IPerson> paircoll =
                new ObjectPairCollection<Agent, IPerson>();
            paircoll.Add(_oldAgent, _newAgent);
            mappedObjectPair.Agent = paircoll;

            ObjectPairCollection<WorktimeType, IContract> paircollContract = new ObjectPairCollection<WorktimeType, IContract>();
            Contract contract = new Contract("foo");
            paircollContract.Add(WorktimeType.Blajj, contract);
            mappedObjectPair.Contract = paircollContract;

            ObjectPairCollection<WorktimeType, IPartTimePercentage> paircollPartTimePercent =
                new ObjectPairCollection<WorktimeType, IPartTimePercentage>();
            PartTimePercentage partTime = new PartTimePercentage("ff");
            paircollPartTimePercent.Add(WorktimeType.Blajj, partTime);
            mappedObjectPair.PartTimePercentage = paircollPartTimePercent;

            ObjectPairCollection<WorktimeType, IContractSchedule> paircollContractSched =
                new ObjectPairCollection<WorktimeType, IContractSchedule>();
            ContractSchedule contractSched = new ContractSchedule("#ff");
            paircollContractSched.Add(WorktimeType.Blajj, contractSched);
            mappedObjectPair.ContractSchedule = paircollContractSched;

            login = ExternalLogOnFactory.CreateExternalLogOn();
            ObjectPairCollection<int, IExternalLogOn> paircollLogin =
                new ObjectPairCollection<int, IExternalLogOn>();
            paircollLogin.Add(-1, login);
            mappedObjectPair.ExternalLogOn = paircollLogin;

            mappedObjectPair.RuleSetBag = new ObjectPairCollection<UnitEmploymentType, IRuleSetBag>();
            
        }


        /// <summary>
        /// Determines whether this instance [can map agent6x].
        /// </summary>
        [Test]
        public void CanMapAgent6XName()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
            //do not test dbid
            Assert.AreEqual("Kalle Kula", newAgent.Name.ToString());
        }

        [Test]
        public void VerifyNullWhenNoPeriods()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgentNoPeriods);
            Assert.IsNull(newAgent);
        }

        [Test]
        public void VerifyTerminalDate()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgentWithNoTerminalDate);
            Assert.IsNull(newAgent.TerminalDate);

            newAgent = _simpleAgentMapper.Map(_oldAgentWithTerminalDate);
            Assert.AreEqual(new DateOnly(new DateTime(2009, 12, 31)), newAgent.TerminalDate);
        }

        //[Test]
        //public void CanMapAgentTerminalDate()
        //{
        //    IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
        //    Assert.AreEqual(new DateOnly(2007, 02, 01), newAgent.TerminalDate);

        //    global::Domain.ICccListCollection<global::Domain.AgentPeriod> agPeriods = new global::Infrastructure.CccListCollection<global::Domain.AgentPeriod>();
        //    global::Domain.DatePeriod datePeriod = new global::Domain.DatePeriod(DateTime.Now);
        //    global::Domain.UnitSub oldTeam = new global::Domain.UnitSub(-1, "Test", -1, false, null);

        //    ICccListCollection<AgentSkill> skillColl = CreateSkillCollection();
        //    agPeriods.Add(new global::Domain.AgentPeriod(datePeriod, -1, null, null, oldTeam, null, null, "", null, skillColl, DateTime.Now));
        //    agPeriods.FinishReadingFromDatabase(global::Domain.CollectionType.Locked);

        //    _oldAgent = new global::Domain.Agent(-1, "Kalle", "Kula", "Kalle@Kula.nu", "", CreateWorkruleCollection(), agPeriods, null, "Test note");
        //    newAgent = _simpleAgentMapper.Map(_oldAgent);
        //    Assert.IsNull(newAgent.TerminalDate);
        //}

        //[Test]
        //public void CanMapAgentTerminalDateWithLocalDate()
        //{
        //    TimeZoneInfo timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        //    _simpleAgentMapper = new AgentMapper(mappedObjectPair, timeZone, new PersonContract(new Contract("test"), new PartTimePercentage("test"), new ContractSchedule("test")));
            
        //    IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
        //    Assert.AreEqual(new DateOnly(2007, 02, 01), newAgent.TerminalDate);
        //}

        [Test]
        public void CanMapPeriodsAndPeriodStartEndTimes()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
            IList<IPersonPeriod> wrapper = new List<IPersonPeriod>(newAgent.PersonPeriodCollection);
            Assert.AreEqual(2, wrapper.Count);
            Assert.AreEqual(wrapper[0].Period.EndDate, wrapper[1].Period.StartDate.AddDays(-1));
            Assert.AreEqual(TimeSpan.Zero, wrapper[0].Period.StartDate.Date.TimeOfDay);
        }

        [Test]
        public void CanMapSchedulePeriods()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
            Assert.AreEqual(2, newAgent.PersonSchedulePeriodCollection.Count);
            Assert.AreEqual(newAgent.PersonSchedulePeriodCollection[0].DateFrom.Date, _oldAgent.WorkruleCollection[0].Period.StartDate);
            Assert.IsTrue(((SchedulePeriod)newAgent.PersonSchedulePeriodCollection[0]).IsDaysOffOverride);
            //Assert.AreEqual(TimeSpan.Zero, newAgent.PersonSchedulePeriodCollection[0].DateFrom.TimeOfDay);
            
            if (_oldAgent.WorkruleCollection[0].WorkloadTypeId == 1)
            {
                Assert.AreEqual(SchedulePeriodType.Day, newAgent.PersonSchedulePeriodCollection[0].PeriodType);
                Assert.AreEqual(newAgent.PersonSchedulePeriodCollection[0].Number, _oldAgent.WorkruleCollection[0].NumberOf);
                Assert.AreEqual(newAgent.PersonSchedulePeriodCollection[0].ShiftCategoryLimitationCollection().Count,_oldAgent.WorkruleCollection[0].CategoryLimitationCollection.Count);
            }
            if (_oldAgent.WorkruleCollection[1].WorkloadTypeId == 2)
            {
                Assert.AreEqual(SchedulePeriodType.Week, newAgent.PersonSchedulePeriodCollection[1].PeriodType);
                Assert.AreEqual(newAgent.PersonSchedulePeriodCollection[1].Number, _oldAgent.WorkruleCollection[1].NumberOf / 7);
                Assert.AreEqual(newAgent.PersonSchedulePeriodCollection[1].ShiftCategoryLimitationCollection().Count, _oldAgent.WorkruleCollection[1].CategoryLimitationCollection.Count);
                Assert.IsFalse(newAgent.PersonSchedulePeriodCollection[1].ShiftCategoryLimitationCollection()[0].Weekly); 
            }
        }


        [Test]
        public void CanMapPersonPeriodNote()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);

            Assert.AreEqual(2, newAgent.PersonPeriodCollection.Count());

            Assert.AreEqual(_note, newAgent.PersonPeriodCollection.First().Note);
        }



        [Test]
        public void CanMapAgentSkill()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
            Assert.AreEqual(_oldAgent.PeriodCollection[0].SkillCollection[0].PeriodSkill.Name,
                            newAgent.PersonPeriodCollection.First().PersonSkillCollection[0].Skill.Name);
            Assert.AreEqual(_oldAgent.PeriodCollection[0].SkillCollection[0].Level.Value,
                            newAgent.PersonPeriodCollection.First().PersonSkillCollection[0].SkillPercentage.Value);
        }

        [Test]
        public void CanMapAgentLogOn()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
            Assert.AreEqual(login,
                            ((PersonPeriod)newAgent.PersonPeriodCollection.First()).ExternalLogOnCollection[0]);
        }

        [Test]
        public void DoNotMapDeletedSkill()
        {
            IPerson newAgent = _simpleAgentMapper.Map(_oldAgent);
            Assert.AreEqual(1, newAgent.PersonPeriodCollection.First().PersonSkillCollection.Count);
        }


        private void SetupObjectPairs()
        {
            mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Team = new ObjectPairCollection<global::Domain.UnitSub, ITeam>();
        }

        private ICccListCollection<AgentSkill> CreateSkillCollection()
        {
	        var type = SkillTypeFactory.CreateSkillType();
            ObjectPairCollection<global::Domain.Skill, ISkill> agentSkillPairs = new ObjectPairCollection<Skill, ISkill>();
            Skill oldSkill =
                new Skill(77, global::Domain.SkillType.InboundTelephony, "theSkill", "", new Color(), null, null, null,
                          false, true, null, null, 3);
			Domain.Forecasting.Skill newSkill = new Domain.Forecasting.Skill("theSkill", "", Color.Red, 15, type);
            ((IEntity)newSkill).SetId(Guid.NewGuid());
            agentSkillPairs.Add(oldSkill, newSkill);

            Skill oldDeletedSkill =
                new Skill(77, global::Domain.SkillType.InboundTelephony, "theSkill", "", new Color(), null, null, null,
                          true, true, null, null, 3);
			Domain.Forecasting.Skill newDeletedSkill = new Domain.Forecasting.Skill("theSkill", "", Color.Red, 15, type);
            ((IEntity)newDeletedSkill).SetId(Guid.NewGuid());
            ((IDeleteTag)newDeletedSkill).SetDeleted();
            agentSkillPairs.Add(oldDeletedSkill, newDeletedSkill);
            mappedObjectPair.Skill = agentSkillPairs;

            ICccListCollection<AgentSkill> skillCollection = new CccListCollection<AgentSkill>();
            skillCollection.Add(new AgentSkill(oldSkill, new SkillLevel("hej", 0.5), true));
            skillCollection.Add(new AgentSkill(oldDeletedSkill, new SkillLevel("hej1", 0.5), true));
            skillCollection.FinishReadingFromDatabase(global::Domain.CollectionType.Locked);

            return skillCollection;
        }

        private static ICccListCollection<AgentWorkrule> CreateWorkruleCollection()
        {
            AgentWorkrule workRule =
                    new AgentWorkrule(
                        new DatePeriod(new DateTime(1900, 1, 1), DateTime.Today.AddDays(1)),
                        new global::Domain.TimePeriod(StandardTimeLimit.WholeDay),
                        new TimeSpan(234),
                        new TimeSpan(123),
                        3, 3, new TimeSpan(1333),
                        WorktimeType.Blajj,
                        new DatePeriod(new DateTime(1907, 1, 1), new DateTime(1908, 3, 3)),
                        new CccListCollection<global::Domain.ShiftCategoryLimitation>(), 1, 4);

            AgentWorkrule workRule2 =
                   new AgentWorkrule(
                       new DatePeriod(DateTime.Today.AddDays(1), DateTime.Today.AddDays(40)),
                       new global::Domain.TimePeriod(StandardTimeLimit.WholeDay),
                       new TimeSpan(234),
                       new TimeSpan(123),
                       3, 3, new TimeSpan(1333),
                       WorktimeType.Blajj,
                       new DatePeriod(DateTime.Today.AddDays(10), DateTime.Today.AddDays(15)),
                       new CccListCollection<global::Domain.ShiftCategoryLimitation>(), 2, 28);

            ICccListCollection<AgentWorkrule> workRuleCollection = new CccListCollection<AgentWorkrule>();
            workRuleCollection.Add(workRule);
            workRuleCollection.Add(workRule2);
            workRuleCollection.FinishReadingFromDatabase(global::Domain.CollectionType.Locked);

            return workRuleCollection;
        }
    }
}
