using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class AdvancedPreferencesPersonalSettingsTest
	{
		private AdvancedPreferencesPersonalSettings _target;
		private IAdvancedPreferences _advancedPreferencesSource;
		private IAdvancedPreferences _advancedPreferencesTarget;

		[SetUp]
		public void Setup()
		{
			_advancedPreferencesSource = new AdvancedPreferences();
			_advancedPreferencesTarget = new AdvancedPreferences();
			_target = new AdvancedPreferencesPersonalSettings();

		}

		[Test]
		public void VerifyDefaultValues()
		{
			_target.MapTo(_advancedPreferencesTarget);
			Assert.AreEqual(TargetValueOptions.StandardDeviation, _advancedPreferencesTarget.TargetValueCalculation);
			Assert.AreEqual(true, _advancedPreferencesTarget.UseMinimumStaffing);
			Assert.AreEqual(true, _advancedPreferencesTarget.UseMaximumStaffing);
			Assert.AreEqual(MaxSeatsFeatureOptions.ConsiderMaxSeats, _advancedPreferencesTarget.UserOptionMaxSeatsFeature);
			Assert.IsTrue(_advancedPreferencesTarget.UseAverageShiftLengths);
			Assert.AreEqual(10, _advancedPreferencesTarget.RefreshScreenInterval);
		}

		[Test]
		public void MappingShouldGetAndSetSimpleProperties()
		{
			_advancedPreferencesSource.TargetValueCalculation = TargetValueOptions.Teleopti;
			_advancedPreferencesSource.UseIntraIntervalDeviation = !_advancedPreferencesSource.UseIntraIntervalDeviation;
			_advancedPreferencesSource.UseTweakedValues = !_advancedPreferencesSource.UseTweakedValues;
			_advancedPreferencesSource.UseMinimumStaffing = !_advancedPreferencesSource.UseMinimumStaffing;
			_advancedPreferencesSource.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats;
			_advancedPreferencesSource.UseMaximumStaffing = !_advancedPreferencesSource.UseMaximumStaffing;
			_advancedPreferencesSource.UseAverageShiftLengths = !_advancedPreferencesSource.UseAverageShiftLengths;
			_advancedPreferencesSource.RefreshScreenInterval = 101;

			_target.MapFrom(_advancedPreferencesSource);
			_target.MapTo(_advancedPreferencesTarget);

			Assert.AreEqual(_advancedPreferencesSource.TargetValueCalculation, _advancedPreferencesTarget.TargetValueCalculation);
			Assert.AreEqual(_advancedPreferencesSource.UseIntraIntervalDeviation, _advancedPreferencesTarget.UseIntraIntervalDeviation);
			Assert.AreEqual(_advancedPreferencesSource.UseTweakedValues, _advancedPreferencesTarget.UseTweakedValues);
			Assert.AreEqual(MaxSeatsFeatureOptions.ConsiderMaxSeats, _advancedPreferencesSource.UserOptionMaxSeatsFeature);
			Assert.AreEqual(_advancedPreferencesSource.UseMinimumStaffing, _advancedPreferencesTarget.UseMinimumStaffing);
			Assert.AreEqual(_advancedPreferencesSource.UseMaximumStaffing, _advancedPreferencesTarget.UseMaximumStaffing);
			Assert.AreEqual(_advancedPreferencesSource.UseAverageShiftLengths, _advancedPreferencesTarget.UseAverageShiftLengths);

			Assert.AreEqual(_advancedPreferencesSource.RefreshScreenInterval, _advancedPreferencesTarget.RefreshScreenInterval);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			_target.MapFrom(null);
			_target.MapTo(null);
		}
	}
}
