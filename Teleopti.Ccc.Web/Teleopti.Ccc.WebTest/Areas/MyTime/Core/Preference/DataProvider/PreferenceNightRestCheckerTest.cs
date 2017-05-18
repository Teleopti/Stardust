using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceNightRestCheckerTest
	{
		private IPerson person;
		private IPersonPreferenceDayOccupationFactory occupationFactory;
		private FakeToggleManager toggleManager;
		private PreferenceNightRestChecker target;

		[SetUp]
		public void Setup()
		{
			var periodStart = new DateOnly(DateTime.Now).AddDays(-7);
			// ExpectedNightRest in the fake person period is 11:00
			person = PersonFactory.CreatePersonWithPersonPeriod(periodStart);

			toggleManager = new FakeToggleManager();
			occupationFactory = MockRepository.GenerateMock<IPersonPreferenceDayOccupationFactory>();
			target = new PreferenceNightRestChecker(occupationFactory, toggleManager);
		}

		[Test]
		public void ShouldHaveNoRestViolationForDateWithToggle43322Disabled()
		{
			toggleManager.Disable(Toggles.MyTimeWeb_PreferencePerformanceForMultipleUsers_43322);

			var currentDate = new DateOnly(DateTime.Now);
			var previousDate = currentDate.AddDays(-1);
			
			occupationFactory.Stub(f => f.GetPreferenceDayOccupation(person, currentDate)).IgnoreArguments()
				.Return(createNormalDayOccupation());

			var result = target.CheckNightRestViolation(person, previousDate);
			Assert.NotNull(result);
			Assert.IsFalse(result.HasViolationToPreviousDay);
			Assert.IsFalse(result.HasViolationToNextDay);

			result = target.CheckNightRestViolation(person, currentDate);
			Assert.NotNull(result);
			Assert.IsFalse(result.HasViolationToPreviousDay);
			Assert.IsFalse(result.HasViolationToNextDay);
		}

		[Test]
		public void ShouldHaveRestViolationForDateWithToggle43322Disabled()
		{
			toggleManager.Disable(Toggles.MyTimeWeb_PreferencePerformanceForMultipleUsers_43322);

			var currentDate = new DateOnly(DateTime.Now);
			var previousDate = currentDate.AddDays(-1);
			var nextDate = currentDate.AddDays(1);

			occupationFactory.Stub(f => f.GetPreferenceDayOccupation(person, previousDate.AddDays(-1)))
				.Return(createNightDayOccupation());
			occupationFactory.Stub(f => f.GetPreferenceDayOccupation(person, previousDate))
				.Return(createNightDayOccupation());
			occupationFactory.Stub(f => f.GetPreferenceDayOccupation(person, currentDate))
				.Return(createNormalDayOccupation());
			occupationFactory.Stub(f => f.GetPreferenceDayOccupation(person, nextDate))
				.Return(createNormalDayOccupation());

			var result = target.CheckNightRestViolation(person, previousDate);
			Assert.NotNull(result);
			Assert.IsFalse(result.HasViolationToPreviousDay);
			Assert.IsTrue(result.HasViolationToNextDay);

			result = target.CheckNightRestViolation(person, currentDate);
			Assert.NotNull(result);
			Assert.IsTrue(result.HasViolationToPreviousDay);
			Assert.IsFalse(result.HasViolationToNextDay);
		}

		[Test]
		public void ShouldHaveNoRestViolationForDate()
		{
			toggleManager.Enable(Toggles.MyTimeWeb_PreferencePerformanceForMultipleUsers_43322);

			var currentDate = new DateOnly(DateTime.Now);
			var previousDate = currentDate.AddDays(-1);
			var nextDate = currentDate.AddDays(1);

			var period = new DateOnlyPeriod(previousDate, currentDate);
			var normalDayOccupation = createNormalDayOccupation();
			occupationFactory.Stub(f => f.GetPreferencePeriodOccupation(person, period)).IgnoreArguments().Return(
				new Dictionary<DateOnly, PersonPreferenceDayOccupation>
				{
					{previousDate.AddDays(-1), normalDayOccupation},
					{previousDate, normalDayOccupation},
					{currentDate, normalDayOccupation},
					{nextDate, normalDayOccupation}
				});

			var result = target.CheckNightRestViolation(person, previousDate);
			Assert.NotNull(result);
			Assert.IsFalse(result.HasViolationToPreviousDay);
			Assert.IsFalse(result.HasViolationToNextDay);

			result = target.CheckNightRestViolation(person, currentDate);
			Assert.NotNull(result);
			Assert.IsFalse(result.HasViolationToPreviousDay);
			Assert.IsFalse(result.HasViolationToNextDay);
		}

		[Test]
		public void ShouldHaveRestViolationForDate()
		{
			toggleManager.Enable(Toggles.MyTimeWeb_PreferencePerformanceForMultipleUsers_43322);

			var currentDate = new DateOnly(DateTime.Now);
			var previousDate = currentDate.AddDays(-1);
			var nextDate = currentDate.AddDays(1);

			var period = new DateOnlyPeriod(previousDate, currentDate);
			occupationFactory.Stub(f => f.GetPreferencePeriodOccupation(person, period)).IgnoreArguments().Return(
				new Dictionary<DateOnly, PersonPreferenceDayOccupation>
				{
					{previousDate.AddDays(-1), createNightDayOccupation()},
					{previousDate, createNightDayOccupation()},
					{currentDate, createNormalDayOccupation()},
					{nextDate, createNormalDayOccupation()}
				});

			var result = target.CheckNightRestViolation(person, previousDate);
			Assert.NotNull(result);
			Assert.IsFalse(result.HasViolationToPreviousDay);
			Assert.IsTrue(result.HasViolationToNextDay);

			result = target.CheckNightRestViolation(person, currentDate);
			Assert.NotNull(result);
			Assert.IsTrue(result.HasViolationToPreviousDay);
			Assert.IsFalse(result.HasViolationToNextDay);
		}

		[Test]
		public void ShouldHaveRestViolationForPeriod()
		{
			var currentDate = new DateOnly(DateTime.Now);
			var previousDate = currentDate.AddDays(-1);
			var nextDate = currentDate.AddDays(1);
			var period = new DateOnlyPeriod(previousDate, currentDate);

			occupationFactory.Stub(f => f.GetPreferencePeriodOccupation(person, period)).IgnoreArguments().Return(
				new Dictionary<DateOnly, PersonPreferenceDayOccupation>
				{
					{previousDate.AddDays(-1), createNightDayOccupation()},
					{previousDate, createNightDayOccupation()},
					{currentDate, createNormalDayOccupation()},
					{nextDate, createNormalDayOccupation()}
				});

			var result = target.CheckNightRestViolation(person, period);

			Assert.AreEqual(result.Count, 2);

			var previousDateCheckResult = result[previousDate];
			Assert.NotNull(previousDateCheckResult);
			Assert.IsFalse(previousDateCheckResult.HasViolationToPreviousDay);
			Assert.IsTrue(previousDateCheckResult.HasViolationToNextDay);

			var currentDateCheckResult = result[currentDate];
			Assert.NotNull(currentDateCheckResult);
			Assert.IsTrue(currentDateCheckResult.HasViolationToPreviousDay);
			Assert.IsFalse(currentDateCheckResult.HasViolationToNextDay);
		}

		[Test]
		public void ShouldHaveNoRestViolationForPeriod()
		{
			var currentDate = new DateOnly(DateTime.Now);
			var previousDate = currentDate.AddDays(-1);
			var nextDate = currentDate.AddDays(1);
			var period = new DateOnlyPeriod(previousDate, currentDate);

			var normalDayOccupation = createNormalDayOccupation();
			occupationFactory.Stub(f => f.GetPreferencePeriodOccupation(person, period)).IgnoreArguments().Return(
				new Dictionary<DateOnly, PersonPreferenceDayOccupation>
				{
					{previousDate.AddDays(-1), normalDayOccupation},
					{previousDate, normalDayOccupation},
					{currentDate, normalDayOccupation},
					{nextDate, normalDayOccupation}
				});

			var result = target.CheckNightRestViolation(person, period);

			Assert.AreEqual(result.Count, 2);

			var previousDateCheckResult = result[previousDate];
			Assert.NotNull(previousDateCheckResult);
			Assert.IsFalse(previousDateCheckResult.HasViolationToPreviousDay);
			Assert.IsFalse(previousDateCheckResult.HasViolationToNextDay);

			var currentDateCheckResult = result[currentDate];
			Assert.NotNull(currentDateCheckResult);
			Assert.IsFalse(currentDateCheckResult.HasViolationToPreviousDay);
			Assert.IsFalse(currentDateCheckResult.HasViolationToNextDay);
		}

		private PersonPreferenceDayOccupation createNormalDayOccupation()
		{
			return new PersonPreferenceDayOccupation
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10)),
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(18)),
				HasFullDayAbsence = false,
				HasDayOff = false,
				HasShift = false,
				HasPreference = true
			};
		}

		private PersonPreferenceDayOccupation createNightDayOccupation()
		{
			return new PersonPreferenceDayOccupation
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(20), TimeSpan.FromHours(22)),
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(28), TimeSpan.FromHours(30)),
				HasFullDayAbsence = false,
				HasDayOff = false,
				HasShift = false,
				HasPreference = true
			};
		}
	}
}
