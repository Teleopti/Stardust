using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
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
			Assert.AreEqual(true, _advancedPreferencesTarget.UseMaximumSeats);
			Assert.AreEqual(10, _advancedPreferencesTarget.RefreshScreenInterval);
		}

		[Test]
		public void MappingShouldGetAndSetSimpleProperties()
		{
			_advancedPreferencesSource.TargetValueCalculation = TargetValueOptions.Teleopti;
			_advancedPreferencesSource.UseIntraIntervalDeviation = !_advancedPreferencesSource.UseIntraIntervalDeviation;
			_advancedPreferencesSource.UseTweakedValues = !_advancedPreferencesSource.UseTweakedValues;

			_advancedPreferencesSource.UseMinimumStaffing = !_advancedPreferencesSource.UseMinimumStaffing;
			_advancedPreferencesSource.UseMaximumStaffing = !_advancedPreferencesSource.UseMaximumStaffing;
			_advancedPreferencesSource.UseMaximumSeats = !_advancedPreferencesSource.UseMaximumSeats;
			_advancedPreferencesSource.DoNotBreakMaximumSeats = !_advancedPreferencesSource.DoNotBreakMaximumSeats;

			_advancedPreferencesSource.RefreshScreenInterval = 101;

			_target.MapFrom(_advancedPreferencesSource);
			_target.MapTo(_advancedPreferencesTarget);

			Assert.AreEqual(_advancedPreferencesSource.TargetValueCalculation, _advancedPreferencesTarget.TargetValueCalculation);
			Assert.AreEqual(_advancedPreferencesSource.UseIntraIntervalDeviation, _advancedPreferencesTarget.UseIntraIntervalDeviation);
			Assert.AreEqual(_advancedPreferencesSource.UseTweakedValues, _advancedPreferencesTarget.UseTweakedValues);

			Assert.AreEqual(_advancedPreferencesSource.UseMinimumStaffing, _advancedPreferencesTarget.UseMinimumStaffing);
			Assert.AreEqual(_advancedPreferencesSource.UseMaximumStaffing, _advancedPreferencesTarget.UseMaximumStaffing);
			Assert.AreEqual(_advancedPreferencesSource.UseMaximumSeats, _advancedPreferencesTarget.UseMaximumSeats);
			Assert.AreEqual(_advancedPreferencesSource.DoNotBreakMaximumSeats, _advancedPreferencesTarget.DoNotBreakMaximumSeats);

			Assert.AreEqual(_advancedPreferencesSource.RefreshScreenInterval, _advancedPreferencesTarget.RefreshScreenInterval);
		}

	}
}
