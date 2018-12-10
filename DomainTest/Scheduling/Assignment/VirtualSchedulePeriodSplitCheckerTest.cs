using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class VirtualSchedulePeriodSplitCheckerTest
    {
        private VirtualSchedulePeriodSplitChecker _target;
        private MockRepository _mock;
        private DateOnlyPeriod _schedulePeriod;
        private DateOnly _schedulePeriodStart;
        private DateOnly _schedulePeriodEnd;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulePeriodStart = new DateOnly(2011, 1, 3);
            _schedulePeriodEnd = new DateOnly(2011, 1, 30);
            _schedulePeriod = new DateOnlyPeriod(_schedulePeriodStart, _schedulePeriodEnd);
            _person = _mock.StrictMock<IPerson>();
            _target = new VirtualSchedulePeriodSplitChecker(_person);   
        }

        [Test]
        public void ShouldNotSplitWhenOnePersonPeriodCoverSchedulePeriod()
        {
            var personPeriod = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);

            using(_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod);
                Expect.Call(personPeriod.StartDate).Return(_schedulePeriodStart);
                Expect.Call(_person.PreviousPeriod(personPeriod)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod)).Return(null);
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());
            }

            using(_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(_schedulePeriod, period);
            }
        }

        [Test]
        public void ShouldSplitWhenPersonPeriodStartGreaterThanSchedulePeriodStart()
        {
            var personPeriod = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);
            var personPeriodStart = new DateOnly(2011, 1, 7);
            var virtualPeriod = new DateOnlyPeriod(personPeriodStart, _schedulePeriodEnd);

            using(_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod);
                Expect.Call(personPeriod.StartDate).Return(personPeriodStart);
                Expect.Call(_person.PreviousPeriod(personPeriod)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod)).Return(null);
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());
            }

            using(_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }
        }

        [Test]
        public void ShouldNotSplitWhenFollowingPersonPeriodsAreNotBreaking()
        {
            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod3 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);
            var personPeriodStart1 = new DateOnly(2011, 1, 1);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var personPeriodStart3 = new DateOnly(2011, 1, 20);
            var personContract = _mock.StrictMock<IPersonContract>();
            var contract = _mock.StrictMock<IContract>();
            var partTimePercentage = _mock.StrictMock<IPartTimePercentage>();
            var contractSchedule = _mock.StrictMock<IContractSchedule>();

            using(_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod1);
                Expect.Call(personPeriod1.StartDate).Return(personPeriodStart1).Repeat.AtLeastOnce();
                Expect.Call(_person.PreviousPeriod(personPeriod1)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod1)).Return(personPeriod2);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personContract.Contract).Return(contract).Repeat.AtLeastOnce();
                Expect.Call(personContract.PartTimePercentage).Return(partTimePercentage).Repeat.AtLeastOnce();
                Expect.Call(personContract.ContractSchedule).Return(contractSchedule).Repeat.AtLeastOnce();
                Expect.Call(personPeriod2.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(_person.NextPeriod(personPeriod2)).Return(personPeriod3);
                Expect.Call(personPeriod3.StartDate).Return(personPeriodStart3).Repeat.AtLeastOnce();
                Expect.Call(personPeriod3.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(_person.NextPeriod(personPeriod3)).Return(null);
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

            }

            using(_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(_schedulePeriod, period);
            }
        }

        [Test]
        public void ShouldSplitOnBreakingContractInFollowingPersonPeriods()
        {
            IContract contract = new Contract("contract");
            IContract contractBreaking = new Contract("breaking");
            IPartTimePercentage partTimePercentage = new PartTimePercentage("partTimePercentage");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");

            IPersonContract personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
            IPersonContract personContractBreaking = new PersonContract(contractBreaking, partTimePercentage, contractSchedule);
            

            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);
            var personPeriodStart1 = new DateOnly(2011, 1, 1);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var virtualPeriod = new DateOnlyPeriod(_schedulePeriodStart, personPeriodStart2.AddDays(-1));

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod1);
                Expect.Call(personPeriod1.StartDate).Return(personPeriodStart1).Repeat.AtLeastOnce();
                Expect.Call(_person.PreviousPeriod(personPeriod1)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod1)).Return(personPeriod2);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personPeriod2.PersonContract).Return(personContractBreaking).Repeat.AtLeastOnce();
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }    
        }

        [Test]
        public void ShouldSplitOnBreakingContractScheduleInFollowingPersonPeriods()
        {
            IContract contract = new Contract("contract");
            IPartTimePercentage partTimePercentage = new PartTimePercentage("partTimePercentage");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");
            IContractSchedule contractScheduleBreaking = new ContractSchedule("breaking");

            IPersonContract personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
            IPersonContract personContractBreaking = new PersonContract(contract, partTimePercentage, contractScheduleBreaking);

            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);
            var personPeriodStart1 = new DateOnly(2011, 1, 1);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var virtualPeriod = new DateOnlyPeriod(_schedulePeriodStart, personPeriodStart2.AddDays(-1));

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod1);
                Expect.Call(personPeriod1.StartDate).Return(personPeriodStart1).Repeat.AtLeastOnce();
                Expect.Call(_person.PreviousPeriod(personPeriod1)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod1)).Return(personPeriod2);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personPeriod2.PersonContract).Return(personContractBreaking).Repeat.AtLeastOnce();
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }
        }

        [Test]
        public void ShouldSplitOnBreakingPartTimePercentageInFollowingPersonPeriods()
        {
            IContract contract = new Contract("contract");
            IPartTimePercentage partTimePercentage = new PartTimePercentage("partTimePercentage");
            IPartTimePercentage partTimePercentageBreaking = new PartTimePercentage("breaking");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");
            

            IPersonContract personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
            IPersonContract personContractBreaking = new PersonContract(contract, partTimePercentageBreaking, contractSchedule);

            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);
            var personPeriodStart1 = new DateOnly(2011, 1, 1);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var virtualPeriod = new DateOnlyPeriod(_schedulePeriodStart, personPeriodStart2.AddDays(-1));

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod1);
                Expect.Call(personPeriod1.StartDate).Return(personPeriodStart1).Repeat.AtLeastOnce();
                Expect.Call(_person.PreviousPeriod(personPeriod1)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod1)).Return(personPeriod2);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personPeriod2.PersonContract).Return(personContractBreaking).Repeat.AtLeastOnce();
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }
        }

        [Test]
        public void ShouldNotSplitWhenPreviousPersonPeriodsAreNotBreaking()
        {
            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod3 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 15);
            var personPeriodStart1 = new DateOnly(2011, 1, 1);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var personPeriodStart3 = new DateOnly(2011, 1, 20);
            var personContract = _mock.StrictMock<IPersonContract>();
            var contractSchedule = _mock.StrictMock<IContractSchedule>();
            var contract = _mock.StrictMock<IContract>();
            var partTimePercentage = _mock.StrictMock<IPartTimePercentage>();

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod3);
                Expect.Call(personPeriod3.StartDate).Return(personPeriodStart3).Repeat.AtLeastOnce();
                Expect.Call(_person.PreviousPeriod(personPeriod3)).Return(personPeriod2);
                Expect.Call(_person.NextPeriod(personPeriod3)).Return(null);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personContract.Contract).Return(contract).Repeat.AtLeastOnce();
                Expect.Call(personContract.PartTimePercentage).Return(partTimePercentage).Repeat.AtLeastOnce();
                Expect.Call(personContract.ContractSchedule).Return(contractSchedule).Repeat.AtLeastOnce();
                Expect.Call(personPeriod2.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(_person.PreviousPeriod(personPeriod2)).Return(personPeriod1);
                Expect.Call(personPeriod3.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(_person.PreviousPeriod(personPeriod1)).Return(null);
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());
                Expect.Call(personPeriod1.StartDate).Return(personPeriodStart1);

            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(_schedulePeriod, period);
            }
        }

        [Test]
        public void ShouldSplitOnBreakingContractInPreviousPersonPeriods()
        {
            IContract contract = new Contract("contract");
            IContract contractBreaking = new Contract("breaking");
            IPartTimePercentage partTimePercentage = new PartTimePercentage("partTimePercentage");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");

            IPersonContract personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
            IPersonContract personContractBreaking = new PersonContract(contractBreaking, partTimePercentage, contractSchedule);

            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 15);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var virtualPeriod = new DateOnlyPeriod(personPeriodStart2, _schedulePeriodEnd);

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod2);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(_person.NextPeriod(personPeriod2)).Return(null);
                Expect.Call(_person.PreviousPeriod(personPeriod2)).Return(personPeriod1);
                Expect.Call(personPeriod2.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContractBreaking).Repeat.AtLeastOnce();
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }
        }

        [Test]
        public void ShouldSplitOnBreakingContractScheduleInPreviousPersonPeriods()
        {
            IContract contract = new Contract("contract");
            IPartTimePercentage partTimePercentage = new PartTimePercentage("partTimePercentage");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");
            IContractSchedule contractScheduleBreaking = new ContractSchedule("breaking");

            IPersonContract personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
            IPersonContract personContractBreaking = new PersonContract(contract, partTimePercentage, contractScheduleBreaking);

            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 15);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var virtualPeriod = new DateOnlyPeriod(personPeriodStart2, _schedulePeriodEnd);

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod2);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(_person.NextPeriod(personPeriod2)).Return(null);
                Expect.Call(_person.PreviousPeriod(personPeriod2)).Return(personPeriod1);
                Expect.Call(personPeriod2.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContractBreaking).Repeat.AtLeastOnce();
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }
        }

        [Test]
        public void ShouldSplitOnBreakingPartTimePercentageInPreviousPersonPeriods()
        {
            IContract contract = new Contract("contract");
            IPartTimePercentage partTimePercentage = new PartTimePercentage("partTimePercentage");
            IPartTimePercentage partTimePercentageBreaking = new PartTimePercentage("breaking");
            IContractSchedule contractSchedule = new ContractSchedule("contractSchedule");

            IPersonContract personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
            IPersonContract personContractBreaking = new PersonContract(contract, partTimePercentageBreaking, contractSchedule);

            var personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            var personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 15);
            var personPeriodStart2 = new DateOnly(2011, 1, 11);
            var virtualPeriod = new DateOnlyPeriod(personPeriodStart2, _schedulePeriodEnd);

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod2);
                Expect.Call(personPeriod2.StartDate).Return(personPeriodStart2).Repeat.AtLeastOnce();
                Expect.Call(_person.NextPeriod(personPeriod2)).Return(null);
                Expect.Call(_person.PreviousPeriod(personPeriod2)).Return(personPeriod1);
                Expect.Call(personPeriod2.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personPeriod1.PersonContract).Return(personContractBreaking).Repeat.AtLeastOnce();
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }
        }

        [Test]
        public void ShouldSplitOnTerminalDate()
        {
            var personPeriod = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);
            var terminalDate = new DateOnly(2011, 1, 20);
            var virtualPeriod = new DateOnlyPeriod(_schedulePeriodStart, terminalDate);

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod);
                Expect.Call(personPeriod.StartDate).Return(_schedulePeriodStart);
                Expect.Call(_person.PreviousPeriod(personPeriod)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod)).Return(null);
                Expect.Call(_person.TerminalDate).Return(terminalDate).Repeat.AtLeastOnce();
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());
            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }   
        }

        [Test]
        public void ShouldSplitOnCollidingSchedulePeriod()
        {
            var personPeriod = _mock.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 10);
            var schedulePeriodStart = new DateOnly(2011, 1, 19);
            var virtualPeriod = new DateOnlyPeriod(_schedulePeriodStart, schedulePeriodStart.AddDays(-1));
            var schedulePeriod = _mock.StrictMock<ISchedulePeriod>();

            using (_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod);
                Expect.Call(personPeriod.StartDate).Return(_schedulePeriodStart);
                Expect.Call(_person.PreviousPeriod(personPeriod)).Return(null);
                Expect.Call(_person.NextPeriod(personPeriod)).Return(null);
                Expect.Call(_person.TerminalDate).Return(null);
                Expect.Call(_person.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>{schedulePeriod});
                Expect.Call(schedulePeriod.DateFrom).Return(schedulePeriodStart).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.AreEqual(virtualPeriod, period);
            }      
        }

        [Test]
        public void ShouldReturnNullWhenNoPersonPeriod()
        {
            var dateOnly = new DateOnly(2010, 12, 30);

            using(_mock.Record())
            {
                Expect.Call(_person.Period(dateOnly)).Return(null);
            }

            using(_mock.Playback())
            {
                var period = _target.Check(_schedulePeriod, dateOnly);
                Assert.IsNull(period);
            }
        }
    }
}
