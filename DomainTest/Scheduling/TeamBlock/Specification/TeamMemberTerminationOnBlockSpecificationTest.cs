using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;


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
		public void ShouldNotLockIfNoMemberIsTerminatedForSingleAgentTeam()
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
				_target.LockTerminatedMembers(_teamInfo, blockInfo);
			}
		}
		
		[Test]
        public void ShouldNotLockIfNoMemberIsTerminated()
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
                _target.LockTerminatedMembers(_teamInfo, blockInfo);
            }
        }

        [Test]
        public void ShouldLockIfMemberIsTerminated()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 03, 30, 2014, 04, 01));
            var terminationDateForPerson1 = new DateOnly(2014, 04, 02);
            var terminationDateForPerson2 = new DateOnly(2014, 03, 30);
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
                Expect.Call(_person1.TerminalDate).Return(terminationDateForPerson1).Repeat.Times(3);
                Expect.Call(_person2.TerminalDate).Return(terminationDateForPerson2).Repeat.Times(3);
				Expect.Call(() => _teamInfo.LockMember(new DateOnlyPeriod(terminationDateForPerson2.AddDays(1), terminationDateForPerson2.AddDays(1)), _person2));
				Expect.Call(() => _teamInfo.LockMember(new DateOnlyPeriod(terminationDateForPerson2.AddDays(2), terminationDateForPerson2.AddDays(2)), _person2));
			}
            using (_mock.Playback())
            {
                _target.LockTerminatedMembers(_teamInfo, blockInfo);
            }
        }

        [Test]
        public void ShouldNotLockOnASingleDayBlockWithTwoNotTerminatedTeamMembers()
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
                _target.LockTerminatedMembers(_teamInfo, blockInfo);
            }
        }

        [Test]
        public void ShouldLockOnASingleDayBlockWithTerminatedTeamMembers()
        {
	        var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 30, 2014, 03, 30);

			var blockInfo = new BlockInfo(dateOnlyPeriod);
            var terminationDateForPerson1 = new DateOnly(2014, 03, 28);
			var terminationDateForPerson2 = new DateOnly(2014, 03, 28);
			using (_mock.Record())
            {
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
                Expect.Call(_person1.TerminalDate).Return(terminationDateForPerson1);
				Expect.Call(_person2.TerminalDate).Return(terminationDateForPerson2);
				Expect.Call(() => _teamInfo.LockMember(dateOnlyPeriod, _person1));
				Expect.Call(() => _teamInfo.LockMember(dateOnlyPeriod, _person2));
			}
            using (_mock.Playback())
            {
                _target.LockTerminatedMembers(_teamInfo, blockInfo);
            }
        }      
    }   
}
