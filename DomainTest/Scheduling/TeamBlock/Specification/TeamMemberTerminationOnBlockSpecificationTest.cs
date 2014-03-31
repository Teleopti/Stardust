using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Specification
{
    [TestFixture]
    public class TeamMemberTerminationOnBlockSpecificationTest
    {
        private ITeamMemberTerminationOnBlockSpecification _target;
        private ITeamInfo _teamInfo;
        private MockRepository _mock;
        private IPerson _person1;
        private IPerson _person2;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _person1 = _mock.StrictMock<IPerson>();
            _person2 = _mock.StrictMock<IPerson>();
            _teamInfo = _mock.StrictMock<ITeamInfo>();
            _target = new TeamMemberTerminationOnBlockSpecification();
        }

        [Test]
        public void ShouldReturnTrueIfNoMemberIsTerminatedForSingleAgentTeam()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 03, 30, 2014, 04, 01));
            var terminationDate = new DateOnly(2014, 04, 02);
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1 });
                Expect.Call(_person1.TerminalDate).Return(terminationDate).Repeat.Times(3);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfy(_teamInfo, blockInfo));    
            }
            
        }

        [Test]
        public void ShouldReturnTrueIfNoMemberIsTerminated()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 03, 30, 2014, 04, 01));
            var terminationDate = new DateOnly(2014, 04, 02);
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1,_person2 });
                Expect.Call(_person1.TerminalDate).Return(terminationDate).Repeat.Times(3);
                Expect.Call(_person2.TerminalDate).Return(terminationDate).Repeat.Times(3);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfy(_teamInfo, blockInfo));
            }

        }

        [Test]
        public void ShouldReturnFalseIfOneOfTheMemberIsTerminated()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 03, 30, 2014, 04, 01));
            var terminationDateForPerson1 = new DateOnly(2014, 04, 02);
            var terminationDateForPerson2 = new DateOnly(2014, 03, 30);
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
                Expect.Call(_person1.TerminalDate).Return(terminationDateForPerson1).Repeat.Times(3);
                Expect.Call(_person2.TerminalDate).Return(terminationDateForPerson2).Repeat.Times(2);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfy(_teamInfo, blockInfo));
            }

        }

        [Test]
        public void ShouldReturnTrueOnASingleDayBlockWithTwoTeamMembers()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 03, 30, 2014, 03, 30));
            var terminationDateForPerson1 = new DateOnly(2014, 04, 02);
            var terminationDateForPerson2 = new DateOnly(2014, 03, 31);
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
                Expect.Call(_person1.TerminalDate).Return(terminationDateForPerson1);
                Expect.Call(_person2.TerminalDate).Return(terminationDateForPerson2);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfy(_teamInfo, blockInfo));
            }

        }

        [Test]
        public void ReturnFalseOnASingleDayBlockWithTwoTeamMembersTerminated()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 03, 30, 2014, 03, 30));
            var terminationDateForPerson1 = new DateOnly(2014, 03, 28);
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
                Expect.Call(_person1.TerminalDate).Return(terminationDateForPerson1);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfy(_teamInfo, blockInfo));
            }

        }
        
    }

    
}
