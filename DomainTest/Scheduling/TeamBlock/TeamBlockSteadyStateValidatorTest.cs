using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockSteadyStateValidatorTest
    {
        private MockRepository _mock;
        private ITeamBlockSteadyStateValidator _target;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private DateOnly _today;
        private ITeamBlockInfo _teamBlockInfo;
        private ISameStartTimeBlockSpecification _sameStartTimeBlockSpecification;
		private ISameEndTimeTeamSpecification _sameEndTimeTeamSpecification;
        private ISameShiftCategoryBlockSpecification _sameShiftCategoryBlockSpecification;
        private ISameShiftBlockSpecification _sameShiftBlockSpecification;
	    private ISameStartTimeTeamSpecification _sameStartTimeTeamSpecification;
	    private ISameShiftCategoryTeamSpecification _sameShiftCategoryTeamSpecification;
	    private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
	    private SchedulingOptions _schedulingOptions;
	    private ITeamBlockOpenHoursValidator _teamBlockOpenHoursValidator;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_sameStartTimeBlockSpecification = _mock.StrictMock<ISameStartTimeBlockSpecification>();
			_sameStartTimeTeamSpecification = _mock.StrictMock<ISameStartTimeTeamSpecification>();
			_sameEndTimeTeamSpecification = _mock.StrictMock<ISameEndTimeTeamSpecification>();
			_sameShiftCategoryBlockSpecification = _mock.StrictMock<ISameShiftCategoryBlockSpecification>();
			_sameShiftCategoryTeamSpecification = _mock.StrictMock<ISameShiftCategoryTeamSpecification>();
			_sameShiftBlockSpecification = _mock.StrictMock<ISameShiftBlockSpecification>();
			_teamBlockSchedulingOptions = _mock.StrictMock<ITeamBlockSchedulingOptions>();
		    _teamBlockOpenHoursValidator = _mock.StrictMock<ITeamBlockOpenHoursValidator>();
		    _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
		    _target = new TeamBlockSteadyStateValidator(_teamBlockSchedulingOptions, _sameStartTimeBlockSpecification,
		                                                _sameStartTimeTeamSpecification, _sameEndTimeTeamSpecification,
		                                                _sameShiftCategoryBlockSpecification,
		                                                _sameShiftCategoryTeamSpecification, _sameShiftBlockSpecification, 
														_teamBlockOpenHoursValidator,
														() => _schedulingResultStateHolder);
		    _schedulingOptions = new SchedulingOptions();
			_today = new DateOnly();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
			_teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
		}

        [Test]
        public void FalseIfSameEndTime()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_sameEndTimeTeamSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameShiftCategoryBlock()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameShiftCategoryBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameShiftCategoryTeam()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameShiftCategoryTeamSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameShiftBlock()
        {
            using (_mock.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameShiftBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(false);
				Expect.Call(_teamBlockOpenHoursValidator.Validate(_teamBlockInfo, _schedulingResultStateHolder)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameStartTimeBlock()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameStartTimeBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void FalseIfSameStartTimeTeam()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameStartTimeTeamSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameEndTeam()
        {
            using (_mock.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_sameEndTimeTeamSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameShiftCategoryTeam()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameShiftCategoryTeamSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameShiftCategoryBlock()
        {
            using (_mock.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameShiftCategoryBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameShiftBlock()
        {
            using (_mock.Record())
            {
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameShiftBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
	            Expect.Call(_teamBlockOpenHoursValidator.Validate(_teamBlockInfo, _schedulingResultStateHolder)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameStartTimeBlock()
        {
			using (_mock.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameStartTimeBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

        [Test]
        public void TrueIfSameStartTimeTeam()
        {
            using (_mock.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameStartTimeTeamSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
            }
        }

		[Test]
		public void ShouldValidateOpenHours()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameStartTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameStartTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShift(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSchedulingWithSameEndTime(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamSameEndTimeInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_sameShiftBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
				Expect.Call(_teamBlockOpenHoursValidator.Validate(_teamBlockInfo, _schedulingResultStateHolder)).Return(false);
			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions));
			}
		}

    }
}