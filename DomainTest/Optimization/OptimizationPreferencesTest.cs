using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.DomainTest.Optimization
{

	[TestFixture]
	public class OptimizationPreferencesTest
	{
		private OptimizationPreferences _target;

		[SetUp]
		public void Setup()
		{
			_target = new OptimizationPreferences();
		}

		[Test]
		public void ShouldAllSubSettingsBeInitializedInConstructor()
		{
			Assert.IsNotNull(_target.General);
			Assert.IsNotNull(_target.Extra);
			Assert.IsNotNull(_target.Advanced);
			Assert.IsNotNull(_target.Rescheduling);
			Assert.IsNotNull(_target.Shifts);
		}

		[Test]
		public void VerifyLocalSchedulingOptionsDefaultValues()
		{
			Assert.IsTrue(_target.Rescheduling.ConsiderShortBreaks);
			Assert.IsTrue(_target.Advanced.UseAverageShiftLengths);
		}

		[Test]
		public void ActivityListInExtraMustNotBeNull()
		{
			Assert.IsNotNull(_target.Shifts.SelectedActivities);
		}
	}
}
