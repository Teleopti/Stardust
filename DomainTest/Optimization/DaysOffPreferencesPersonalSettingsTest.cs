using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

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
	}
}
