using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class DaysOffPreferencesPersonalSettingsTest
	{
		private DaysOffPreferencesPersonalSettings _target;
		private IDaysOffPreferences _daysOffPreferencesSource;
		private IDaysOffPreferences _daysOffPreferencesTarget;

		[SetUp]
		public void Setup()
		{
			_daysOffPreferencesSource = new DaysOffPreferences();
			_daysOffPreferencesTarget = new DaysOffPreferences();
			_target = new DaysOffPreferencesPersonalSettings();
		}

		[Test]
		public void VerifyDefaultValues()
		{
			_target.MapTo(_daysOffPreferencesTarget);
			Assert.AreEqual(0d, _daysOffPreferencesTarget.KeepExistingDaysOffValue);

			Assert.AreEqual(true, _daysOffPreferencesTarget.UseDaysOffPerWeek);
			Assert.AreEqual(true, _daysOffPreferencesTarget.UseConsecutiveDaysOff);
			Assert.AreEqual(true, _daysOffPreferencesTarget.UseConsecutiveWorkdays);

			Assert.AreEqual(1, _daysOffPreferencesTarget.DaysOffPerWeekValue.Minimum);
			Assert.AreEqual(3, _daysOffPreferencesTarget.DaysOffPerWeekValue.Maximum);
			Assert.AreEqual(1, _daysOffPreferencesTarget.ConsecutiveDaysOffValue.Minimum);
			Assert.AreEqual(3, _daysOffPreferencesTarget.ConsecutiveDaysOffValue.Maximum);
			Assert.AreEqual(2, _daysOffPreferencesTarget.ConsecutiveWorkdaysValue.Minimum);
			Assert.AreEqual(6, _daysOffPreferencesTarget.ConsecutiveWorkdaysValue.Maximum);
		}

		[Test]
		public void MapToShouldGetAndSetMinMaxProperties()
		{
			_daysOffPreferencesSource.DaysOffPerWeekValue = new MinMax<int>(8, 9);
			_daysOffPreferencesSource.ConsecutiveDaysOffValue = new MinMax<int>(10, 11);
			_daysOffPreferencesSource.ConsecutiveWorkdaysValue = new MinMax<int>(12, 13);
			_daysOffPreferencesSource.FullWeekendsOffValue = new MinMax<int>(14, 15);
			_daysOffPreferencesSource.WeekEndDaysOffValue = new MinMax<int>(14, 15);
			_target.MapFrom(_daysOffPreferencesSource);
			_target.MapTo(_daysOffPreferencesTarget);
			Assert.AreEqual(_daysOffPreferencesSource.DaysOffPerWeekValue.Minimum, _daysOffPreferencesTarget.DaysOffPerWeekValue.Minimum);
			Assert.AreEqual(_daysOffPreferencesSource.DaysOffPerWeekValue.Maximum, _daysOffPreferencesTarget.DaysOffPerWeekValue.Maximum);
			Assert.AreEqual(_daysOffPreferencesSource.ConsecutiveDaysOffValue.Minimum, _daysOffPreferencesTarget.ConsecutiveDaysOffValue.Minimum);
			Assert.AreEqual(_daysOffPreferencesSource.ConsecutiveDaysOffValue.Maximum, _daysOffPreferencesTarget.ConsecutiveDaysOffValue.Maximum);
			Assert.AreEqual(_daysOffPreferencesSource.ConsecutiveWorkdaysValue.Minimum, _daysOffPreferencesTarget.ConsecutiveWorkdaysValue.Minimum);
			Assert.AreEqual(_daysOffPreferencesSource.ConsecutiveWorkdaysValue.Maximum, _daysOffPreferencesTarget.ConsecutiveWorkdaysValue.Maximum);
			Assert.AreEqual(_daysOffPreferencesSource.FullWeekendsOffValue.Minimum, _daysOffPreferencesTarget.FullWeekendsOffValue.Minimum);
			Assert.AreEqual(_daysOffPreferencesSource.FullWeekendsOffValue.Maximum, _daysOffPreferencesTarget.FullWeekendsOffValue.Maximum);
			Assert.AreEqual(_daysOffPreferencesSource.WeekEndDaysOffValue.Minimum, _daysOffPreferencesTarget.WeekEndDaysOffValue.Minimum);
			Assert.AreEqual(_daysOffPreferencesSource.WeekEndDaysOffValue.Maximum, _daysOffPreferencesTarget.WeekEndDaysOffValue.Maximum);
		}

		[Test]
		public void MapToShouldGetAndSetSimpleProperties()
		{
			_daysOffPreferencesSource.UseKeepExistingDaysOff = !_daysOffPreferencesSource.UseKeepExistingDaysOff;
			_daysOffPreferencesSource.KeepExistingDaysOffValue = 1000d;
			_daysOffPreferencesSource.UseDaysOffPerWeek = !_daysOffPreferencesSource.UseDaysOffPerWeek;
			_daysOffPreferencesSource.UseConsecutiveDaysOff = !_daysOffPreferencesSource.UseConsecutiveDaysOff;
			_daysOffPreferencesSource.UseConsecutiveWorkdays = !_daysOffPreferencesSource.UseConsecutiveWorkdays;
			_daysOffPreferencesSource.UseFullWeekendsOff = !_daysOffPreferencesSource.UseFullWeekendsOff;
			_daysOffPreferencesSource.UseWeekEndDaysOff = !_daysOffPreferencesSource.UseWeekEndDaysOff;
			_daysOffPreferencesSource.ConsiderWeekBefore = !_daysOffPreferencesSource.ConsiderWeekBefore;
			_daysOffPreferencesSource.ConsiderWeekAfter = !_daysOffPreferencesSource.ConsiderWeekAfter;
			_daysOffPreferencesSource.KeepFreeWeekends = !_daysOffPreferencesSource.KeepFreeWeekends;
			_daysOffPreferencesSource.KeepFreeWeekendDays = !_daysOffPreferencesSource.KeepFreeWeekendDays;
			_target.MapFrom(_daysOffPreferencesSource);
			_target.MapTo(_daysOffPreferencesTarget);
			Assert.AreEqual(_daysOffPreferencesSource.UseKeepExistingDaysOff, _daysOffPreferencesTarget.UseKeepExistingDaysOff);
			Assert.AreEqual(_daysOffPreferencesSource.KeepExistingDaysOffValue, _daysOffPreferencesTarget.KeepExistingDaysOffValue);
			Assert.AreEqual(_daysOffPreferencesSource.UseDaysOffPerWeek, _daysOffPreferencesTarget.UseDaysOffPerWeek);
			Assert.AreEqual(_daysOffPreferencesSource.UseConsecutiveDaysOff, _daysOffPreferencesTarget.UseConsecutiveDaysOff);
			Assert.AreEqual(_daysOffPreferencesSource.UseConsecutiveWorkdays, _daysOffPreferencesTarget.UseConsecutiveWorkdays);
			Assert.AreEqual(_daysOffPreferencesSource.UseFullWeekendsOff, _daysOffPreferencesTarget.UseFullWeekendsOff);
			Assert.AreEqual(_daysOffPreferencesSource.UseWeekEndDaysOff, _daysOffPreferencesTarget.UseWeekEndDaysOff);
			Assert.AreEqual(_daysOffPreferencesSource.ConsiderWeekBefore, _daysOffPreferencesTarget.ConsiderWeekBefore);
			Assert.AreEqual(_daysOffPreferencesSource.ConsiderWeekAfter, _daysOffPreferencesTarget.ConsiderWeekAfter);
			Assert.AreEqual(_daysOffPreferencesSource.KeepFreeWeekends, _daysOffPreferencesTarget.KeepFreeWeekends);
			Assert.AreEqual(_daysOffPreferencesSource.KeepFreeWeekendDays, _daysOffPreferencesTarget.KeepFreeWeekendDays);
		}
	}
}
