using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockSchedulingOptionsTest
	{
		private ITeamBlockSchedulingOptions _target;
		private SchedulingOptions _schedulingOptions;

		[SetUp]
		public void Setup()
		{
			_target = new TeamBlockSchedulingOptions();
			_schedulingOptions = new SchedulingOptions();
		}

		[Test]
		public void ShouldBeBlockScheduling()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = false;

			var result = _target.IsBlockScheduling(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldBeTeamScheduling()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;

			var result = _target.IsTeamScheduling(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldBeTeamBlockScheduling()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;

			var result = _target.IsTeamBlockScheduling(_schedulingOptions);
			Assert.IsTrue(result);

			result = _target.IsBlockScheduling(_schedulingOptions);
			Assert.IsFalse(result);
			result = _target.IsTeamScheduling(_schedulingOptions);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouleBeBlockSchedulingWithSameShift()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = false;
			_schedulingOptions.BlockSameShift = true;

			var result = _target.IsBlockSchedulingWithSameShift(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeBlockSchedulingWithSameShiftCategory()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = false;
			_schedulingOptions.BlockSameShiftCategory = true;

			var result = _target.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeBlockSchedulingWithSameStartTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = false;
			_schedulingOptions.BlockSameStartTime = true;

			var result = _target.IsBlockSchedulingWithSameStartTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameStartTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameStartTime = true;

			var result = _target.IsTeamSchedulingWithSameStartTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameEndTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameEndTime = true;

			var result = _target.IsTeamSchedulingWithSameEndTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameShiftCategory()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameShiftCategory = true;

			var result = _target.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithTeamSameShiftCategory()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameShiftCategory = true;

			var result = _target.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithTeamSameStartTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameStartTime = true;

			var result = _target.IsTeamSameStartTimeInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithTeamSameEndTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameEndTime = true;

			var result = _target.IsTeamSameEndTimeInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameStartTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.BlockSameStartTime = true;

			var result = _target.IsBlockSameStartTimeInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameShift()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.BlockSameShift = true;

			var result = _target.IsBlockSameShiftInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameShiftCategory()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.BlockSameShiftCategory = true;

			var result = _target.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithBlockSameActivity()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameActivity = true;

			var result = _target.IsTeamSchedulingWithSameActivity(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameActivity()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.TeamSameActivity = true;

			var result = _target.IsTeamSameActivityInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

        [Test]
        public void ShouldReturnFalseIfNotSingleAgentTeam()
        {
            IGroupPageLight singleAgentTeam = new GroupPageLight();
            singleAgentTeam.Key = "BusinessUnit";
            singleAgentTeam.Name = "BusinessUnit";
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = singleAgentTeam;
            var result = _target.IsSingleAgentTeam(_schedulingOptions);
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldReturnTrueIfSingleAgentTeam()
        {
            IGroupPageLight singleAgentTeam = new GroupPageLight();
            singleAgentTeam.Key = "SingleAgentTeam";
            singleAgentTeam.Name = "SingleAgentTeam";
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = singleAgentTeam;
            var result = _target.IsSingleAgentTeam(_schedulingOptions);
            Assert.IsTrue(result);
        }

		[Test]
		public void IsBlockWithSameShiftCategoryInvolvedShouldReturnTrueIfBlockIsNotSingleDay()
		{
			_schedulingOptions.BlockSameShiftCategory = false;
			Assert.IsFalse(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));

			_schedulingOptions.BlockSameShiftCategory = true;

			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.None;
			Assert.IsFalse(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));

			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			Assert.IsFalse(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));

			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
			Assert.IsTrue(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));

			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
			Assert.IsTrue(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));
		}
	}
}
