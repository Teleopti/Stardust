using NUnit.Framework;
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
			_schedulingOptions.UseTeamBlockSameShift = true;
			_schedulingOptions.UseTeamBlockSameShiftCategory = false;
			_schedulingOptions.UseTeamBlockSameStartTime = false;
			_schedulingOptions.UseTeamBlockSameEndTime = false;

			var result = _target.IsBlockSchedulingWithSameShift(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeBlockSchedulingWithSameShiftCategory()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = false;
			_schedulingOptions.UseTeamBlockSameShift = false;
			_schedulingOptions.UseTeamBlockSameShiftCategory = true;
			_schedulingOptions.UseTeamBlockSameStartTime = false;
			_schedulingOptions.UseTeamBlockSameEndTime = false;

			var result = _target.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeBlockSchedulingWithSameStartTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = true;
			_schedulingOptions.UseGroupScheduling = false;
			_schedulingOptions.UseTeamBlockSameShift = false;
			_schedulingOptions.UseTeamBlockSameShiftCategory = false;
			_schedulingOptions.UseTeamBlockSameStartTime = true;
			_schedulingOptions.UseTeamBlockSameEndTime = false;

			var result = _target.IsBlockSchedulingWithSameStartTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameStartTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.UseGroupSchedulingCommonStart = true;
			_schedulingOptions.UseGroupSchedulingCommonCategory = false;
			_schedulingOptions.UseGroupSchedulingCommonEnd = false;

			var result = _target.IsTeamSchedulingWithSameStartTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameEndTime()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.UseGroupSchedulingCommonStart = false;
			_schedulingOptions.UseGroupSchedulingCommonCategory = false;
			_schedulingOptions.UseGroupSchedulingCommonEnd = true;

			var result = _target.IsTeamSchedulingWithSameEndTime(_schedulingOptions);

			Assert.IsTrue(result);
		}

		[Test]
		public void ShouleBeTeamSchedulingWithSameShiftCategory()
		{
			_schedulingOptions.UseTeamBlockPerOption = false;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.UseGroupSchedulingCommonStart = false;
			_schedulingOptions.UseGroupSchedulingCommonCategory = true;
			_schedulingOptions.UseGroupSchedulingCommonEnd = false;

			var result = _target.IsTeamSchedulingWithSameShiftCategory(_schedulingOptions);

			Assert.IsTrue(result);
		}
	}
}
