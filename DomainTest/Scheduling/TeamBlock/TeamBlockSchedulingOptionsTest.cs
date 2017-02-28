﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

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
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = false;

			var result = _target.IsBlockScheduling(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldBeTeamScheduling()
		{
			_schedulingOptions.UseBlock = false;
			_schedulingOptions.UseTeam = true;

			var result = _target.IsTeamScheduling(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldBeTeamBlockScheduling()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;

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
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = false;
			_schedulingOptions.BlockSameShift = true;

			var result = _target.IsBlockSchedulingWithSameShift(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeBlockSchedulingWithSameShiftCategory()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = false;
			_schedulingOptions.BlockSameShiftCategory = true;

			var result = _target.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeBlockSchedulingWithSameStartTime()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = false;
			_schedulingOptions.BlockSameStartTime = true;

			var result = _target.IsBlockSchedulingWithSameStartTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameStartTime()
		{
			_schedulingOptions.UseBlock = false;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameStartTime = true;

			var result = _target.IsTeamSchedulingWithSameStartTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameEndTime()
		{
			_schedulingOptions.UseBlock = false;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameEndTime = true;

			var result = _target.IsTeamSchedulingWithSameEndTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameShiftCategory()
		{
			_schedulingOptions.UseBlock = false;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameShiftCategory = true;

			var result = _target.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithTeamSameShiftCategory()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameShiftCategory = true;

			var result = _target.IsTeamSameShiftCategoryInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithTeamSameStartTime()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameStartTime = true;

			var result = _target.IsTeamSameStartTimeInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithTeamSameEndTime()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameEndTime = true;

			var result = _target.IsTeamSameEndTimeInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameStartTime()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.BlockSameStartTime = true;

			var result = _target.IsBlockSameStartTimeInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameShift()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.BlockSameShift = true;

			var result = _target.IsBlockSameShiftInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameShiftCategory()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.BlockSameShiftCategory = true;

			var result = _target.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithBlockSameActivity()
		{
			_schedulingOptions.UseBlock = false;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameActivity = true;

			var result = _target.IsTeamSchedulingWithSameActivity(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamBlockSchedulingWithBlockSameActivity()
		{
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.UseTeam = true;
			_schedulingOptions.TeamSameActivity = true;

			var result = _target.IsTeamSameActivityInTeamBlock(_schedulingOptions);

			Assert.IsTrue(result);
		}

        [Test]
        public void ShouldReturnFalseIfNotSingleAgentTeam()
        {
	        GroupPageLight singleAgentTeam = new GroupPageLight(string.Empty, GroupPageType.Hierarchy);
            _schedulingOptions.GroupOnGroupPageForTeamBlockPer = singleAgentTeam;
            var result = _target.IsSingleAgentTeam(_schedulingOptions);
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldReturnTrueIfSingleAgentTeam()
        {
            GroupPageLight singleAgentTeam = GroupPageLight.SingleAgentGroup(string.Empty);
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

			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			Assert.IsFalse(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));

			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
			Assert.IsTrue(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));

			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SchedulePeriod;
			Assert.IsTrue(_target.IsBlockWithSameShiftCategoryInvolved(_schedulingOptions));
		}
	}
}
