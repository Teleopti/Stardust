using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockSteadyStateValidatorTest
    {
        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _teamBlockSameStartTimeSpecification = _mock.StrictMock<ITeamBlockSameStartTimeSpecification>();
            _teamBlockSameEndTimeSpecification = _mock.StrictMock<ITeamBlockSameEndTimeSpecification>();
            _teamBlockSameShiftCategorySpecification = _mock.StrictMock<ITeamBlockSameShiftCategorySpecification>();
            _teamBlockSameShiftSpecification = _mock.StrictMock<ITeamBlockSameShiftSpecification>();
            _target = new TeamBlockSteadyStateValidator(_teamBlockSameStartTimeSpecification,
                                                        _teamBlockSameEndTimeSpecification,
                                                        _teamBlockSameShiftCategorySpecification,
                                                        _teamBlockSameShiftSpecification);
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _today = new DateOnly();
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = _mock.StrictMock<ITeamInfo>();
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            _teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
        }

        private MockRepository _mock;
        private ITeamBlockSteadyStateValidator _target;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private DateOnly _today;
        private ITeamBlockInfo _teamBlockInfo;
        private ITeamBlockSameStartTimeSpecification _teamBlockSameStartTimeSpecification;
        private ITeamBlockSameEndTimeSpecification _teamBlockSameEndTimeSpecification;
        private ITeamBlockSameShiftCategorySpecification _teamBlockSameShiftCategorySpecification;
        private ITeamBlockSameShiftSpecification _teamBlockSameShiftSpecification;

        [Test]
        public void FalseIfSameEndTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameEndTime = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameEndTimeSpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameShiftCategoryTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameShiftCategory = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameShiftSpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameShiftTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameShift = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameShiftCategorySpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameStartTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameStartTime = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameStartTimeSpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameEndTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameEndTime = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameEndTimeSpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameShiftCategoryTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameShiftCategory = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameShiftSpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameShiftTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameShift = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameShiftCategorySpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameStartTime()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions {UseTeamBlockSameStartTime = true};
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSameStartTimeSpecification.IsSatisfiedBy(_teamBlockInfo))
                      .IgnoreArguments()
                      .Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsBlockInSteadyState(_teamBlockInfo, schedulingOptions));
            }
        }
    }
}