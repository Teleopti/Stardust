using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class DaysOnlyHelperTest
	{
		private SchedulingOptions _schedulingOptions;
		private DaysOnlyHelper _daysOnlyHelper;

		[SetUp]
		public void Setup()
		{
			_schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				UseRotations = true,
				UseAvailability = true
			};
			_daysOnlyHelper = new DaysOnlyHelper(_schedulingOptions);
		}

		[Test]
		public void ShouldReturnFalseWhenNoDaysOnlyInSchedulingOptions()
		{
			Assert.IsFalse(_daysOnlyHelper.DaysOnly);
		}

		[Test]
		public void ShouldReturnTrueWhenPreferenceWithDaysOnly()
		{
			_schedulingOptions.PreferencesDaysOnly = true;
			Assert.IsTrue(_daysOnlyHelper.DaysOnly);
		}

		[Test]
		public void ShouldReturnTrueWhenPreferenceWithMustHaveOnly()
		{
			_schedulingOptions.UsePreferencesMustHaveOnly = true;
			Assert.IsTrue(_daysOnlyHelper.DaysOnly);
		}

		[Test]
		public void ShouldReturnTrueWhenRotationWithDaysOnly()
		{
			_schedulingOptions.RotationDaysOnly = true;
			Assert.IsTrue(_daysOnlyHelper.DaysOnly);
		}

		[Test]
		public void ShouldReturnTrueWhenAvailabilityWithDaysOnly()
		{
			_schedulingOptions.AvailabilityDaysOnly = true;
			Assert.IsTrue(_daysOnlyHelper.DaysOnly);
		}



		[Test]
		public void ShouldReturnUsePreferencesWithNoDaysOnly()
		{
			_schedulingOptions.PreferencesDaysOnly = true;
			Assert.IsFalse(_daysOnlyHelper.UsePreferencesWithNoDaysOnly);

			_schedulingOptions.PreferencesDaysOnly = false;
			Assert.IsTrue(_daysOnlyHelper.UsePreferencesWithNoDaysOnly);

			_schedulingOptions.UsePreferencesMustHaveOnly = true;
			Assert.IsFalse(_daysOnlyHelper.UsePreferencesWithNoDaysOnly);

			_schedulingOptions.UsePreferencesMustHaveOnly = false;
			Assert.IsTrue(_daysOnlyHelper.UsePreferencesWithNoDaysOnly);
		}


		[Test]
		public void ShouldReturnUseRotationsWithNoDaysOnly()
		{
			_schedulingOptions.RotationDaysOnly = true;
			Assert.IsFalse(_daysOnlyHelper.UseRotationsWithNoDaysOnly);

			_schedulingOptions.RotationDaysOnly = false;
			Assert.IsTrue(_daysOnlyHelper.UseRotationsWithNoDaysOnly);
		}

		[Test]
		public void ShouldReturnUseAvailabilityWithNoDaysOnly()
		{
			_schedulingOptions.AvailabilityDaysOnly = true;
			Assert.IsFalse(_daysOnlyHelper.UseAvailabilityWithNoDaysOnly);

			_schedulingOptions.AvailabilityDaysOnly = false;
			Assert.IsTrue(_daysOnlyHelper.UseAvailabilityWithNoDaysOnly);
		}



		[Test]
		public void ShouldReturnSchedulingOptionsForPreferenceOnlyOptions()
		{
			_schedulingOptions.UsePreferences = true;
			_schedulingOptions.PreferencesDaysOnly = true;
			_schedulingOptions.UseRotations = true;
			_schedulingOptions.RotationDaysOnly = true;
			_schedulingOptions.UseAvailability = true;
			_schedulingOptions.AvailabilityDaysOnly = true;
			_schedulingOptions.UsePreferencesMustHaveOnly = true;

			var options = _daysOnlyHelper.PreferenceOnlyOptions;
			Assert.IsTrue(options.UsePreferences);
			Assert.IsTrue(options.PreferencesDaysOnly);
			Assert.IsTrue(options.UsePreferencesMustHaveOnly);

			Assert.IsFalse(options.UseRotations);
			Assert.IsFalse(options.RotationDaysOnly);
			Assert.IsFalse(options.UseAvailability);
			Assert.IsFalse(options.AvailabilityDaysOnly);
		}

		[Test]
		public void ShouldReturnSchedulingOptionsForRotationOnlyOptions()
		{
			_schedulingOptions.UsePreferences = true;
			_schedulingOptions.PreferencesDaysOnly = true;
			_schedulingOptions.UseRotations = true;
			_schedulingOptions.RotationDaysOnly = true;
			_schedulingOptions.UseAvailability = true;
			_schedulingOptions.AvailabilityDaysOnly = true;
			_schedulingOptions.UsePreferencesMustHaveOnly = true;

			var options = _daysOnlyHelper.RotationOnlyOptions;
			Assert.IsTrue(options.UseRotations);
			Assert.IsTrue(options.RotationDaysOnly);

			Assert.IsFalse(options.UsePreferencesMustHaveOnly);
			Assert.IsFalse(options.UsePreferences);
			Assert.IsFalse(options.PreferencesDaysOnly);
			Assert.IsFalse(options.UseAvailability);
			Assert.IsFalse(options.AvailabilityDaysOnly);
		}

		[Test]
		public void ShouldReturnSchedulingOptionsForAvailabilityOnlyOptions()
		{
			_schedulingOptions.UsePreferences = true;
			_schedulingOptions.PreferencesDaysOnly = true;
			_schedulingOptions.UseRotations = true;
			_schedulingOptions.RotationDaysOnly = true;
			_schedulingOptions.UseAvailability = true;
			_schedulingOptions.AvailabilityDaysOnly = true;
			_schedulingOptions.UsePreferencesMustHaveOnly = true;

			var options = _daysOnlyHelper.AvailabilityOnlyOptions;
			Assert.IsTrue(options.UseAvailability);
			Assert.IsTrue(options.AvailabilityDaysOnly);

			Assert.IsFalse(options.UseRotations);
			Assert.IsFalse(options.RotationDaysOnly);
			Assert.IsFalse(options.UsePreferencesMustHaveOnly);
			Assert.IsFalse(options.UsePreferences);
			Assert.IsFalse(options.PreferencesDaysOnly);
			
		}

		[Test]
		public void ShouldReturnWithNoDaysOnlyOptions()
		{
			_schedulingOptions.UsePreferences = true;
			_schedulingOptions.PreferencesDaysOnly = true;
			_schedulingOptions.UseRotations = true;
			_schedulingOptions.RotationDaysOnly = true;
			_schedulingOptions.UseAvailability = true;
			_schedulingOptions.AvailabilityDaysOnly = true;
			_schedulingOptions.UsePreferencesMustHaveOnly = true;

			var options = _daysOnlyHelper.NoOnlyOptions;
			Assert.IsTrue(options.UsePreferences);
			Assert.IsTrue(options.UseRotations);
			Assert.IsTrue(options.UseAvailability);

			Assert.IsFalse(options.PreferencesDaysOnly);
			Assert.IsFalse(options.UsePreferencesMustHaveOnly);
			Assert.IsFalse(options.RotationDaysOnly);
			Assert.IsFalse(options.AvailabilityDaysOnly);		
		}
	}
}
