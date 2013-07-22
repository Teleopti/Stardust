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
	public class SchedulingOptionsDayOffPlannerPersonalSettingsTest
	{
		private SchedulingOptionsDayOffPlannerPersonalSettings _target;
		private DaysOffPreferences _daysOffPreferences;

		[SetUp]
		public void Setup()
		{
			_target = new SchedulingOptionsDayOffPlannerPersonalSettings();
			_daysOffPreferences = new DaysOffPreferences();
			_daysOffPreferences.UseDaysOffPerWeek = true;
			_daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(1,2);

			_daysOffPreferences.UseConsecutiveDaysOff = true;
			_daysOffPreferences.ConsecutiveDaysOffValue = new MinMax<int>(2,3);

			_daysOffPreferences.UseConsecutiveWorkdays = true;
			_daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(3,4);

			_daysOffPreferences.UseFullWeekendsOff = true;
			_daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(5,6);

			_daysOffPreferences.UseWeekEndDaysOff = true;
			_daysOffPreferences.WeekEndDaysOffValue = new MinMax<int>(7,8);

			_daysOffPreferences.ConsiderWeekBefore = true;
			_daysOffPreferences.ConsiderWeekAfter = true;
		}

		[Test]
		public void ShouldHaveDefaultValues()
		{
			var dayOffPreferences = new DaysOffPreferences();
			var settings = new SchedulingOptionsDayOffPlannerPersonalSettings();

			settings.MapTo(dayOffPreferences);

			Assert.IsTrue(dayOffPreferences.UseDaysOffPerWeek);
			Assert.AreEqual(1, dayOffPreferences.DaysOffPerWeekValue.Minimum);
			Assert.AreEqual(3, dayOffPreferences.DaysOffPerWeekValue.Maximum);

			Assert.IsTrue(dayOffPreferences.UseConsecutiveDaysOff);
			Assert.AreEqual(1, dayOffPreferences.ConsecutiveDaysOffValue.Minimum);
			Assert.AreEqual(3, dayOffPreferences.ConsecutiveDaysOffValue.Maximum);

			Assert.IsTrue(dayOffPreferences.UseConsecutiveWorkdays);
			Assert.AreEqual(1, dayOffPreferences.ConsecutiveWorkdaysValue.Minimum);
			Assert.AreEqual(6, dayOffPreferences.ConsecutiveWorkdaysValue.Maximum);
		}

		[Test]
		public void ShouldMap()
		{
			_target.MapFrom(_daysOffPreferences);

			var dayOffPreferences = new DaysOffPreferences();

			_target.MapTo(dayOffPreferences);

			Assert.IsTrue(dayOffPreferences.UseDaysOffPerWeek);
			Assert.AreEqual(dayOffPreferences.DaysOffPerWeekValue.Minimum, _daysOffPreferences.DaysOffPerWeekValue.Minimum);
			Assert.AreEqual(dayOffPreferences.DaysOffPerWeekValue.Maximum, _daysOffPreferences.DaysOffPerWeekValue.Maximum);

			Assert.IsTrue(dayOffPreferences.UseConsecutiveDaysOff);
			Assert.AreEqual(dayOffPreferences.ConsecutiveDaysOffValue.Minimum, _daysOffPreferences.ConsecutiveDaysOffValue.Minimum);
			Assert.AreEqual(dayOffPreferences.ConsecutiveDaysOffValue.Maximum, _daysOffPreferences.ConsecutiveDaysOffValue.Maximum);

			Assert.IsTrue(dayOffPreferences.UseConsecutiveWorkdays);
			Assert.AreEqual(dayOffPreferences.ConsecutiveWorkdaysValue.Minimum, _daysOffPreferences.ConsecutiveWorkdaysValue.Minimum);
			Assert.AreEqual(dayOffPreferences.ConsecutiveWorkdaysValue.Maximum, _daysOffPreferences.ConsecutiveWorkdaysValue.Maximum);

			Assert.IsTrue(dayOffPreferences.UseFullWeekendsOff);
			Assert.AreEqual(dayOffPreferences.FullWeekendsOffValue.Minimum, _daysOffPreferences.FullWeekendsOffValue.Minimum);
			Assert.AreEqual(dayOffPreferences.FullWeekendsOffValue.Maximum, _daysOffPreferences.FullWeekendsOffValue.Maximum);

			Assert.IsTrue(dayOffPreferences.UseWeekEndDaysOff);
			Assert.AreEqual(dayOffPreferences.WeekEndDaysOffValue.Minimum, _daysOffPreferences.WeekEndDaysOffValue.Minimum);
			Assert.AreEqual(dayOffPreferences.WeekEndDaysOffValue.Maximum, _daysOffPreferences.WeekEndDaysOffValue.Maximum);

			Assert.IsTrue(dayOffPreferences.ConsiderWeekBefore);
			Assert.IsTrue(dayOffPreferences.ConsiderWeekAfter);
		}
	}
}
