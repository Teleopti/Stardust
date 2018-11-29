using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.RestrictionSummary
{
	[TestFixture]
	public class PreferenceNightRestCheckerTest
	{
		private MockRepository _mocks;
		private IEffectiveRestriction _effectiveRestriction;
		private PreferenceNightRestChecker _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_target = new PreferenceNightRestChecker();
		}

		[Test]
		public void ShouldSetViolatesIfTooShortNightRest()
		{
			var date1 = new DateOnly(2011, 9, 1);
            var date2 = new DateOnly(2011, 9, 2);
            var date3 = new DateOnly(2011, 9, 3);
			var endTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(21), TimeSpan.FromHours(23));
			var startTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(8));
			var cellData1 = new PreferenceCellData {EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1};
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			var cellData3 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date3 };
			var cellDatas = new Dictionary<int, IPreferenceCellData>{{1,cellData1}, {2,cellData2},{3,cellData3}};

			Expect.Call(_effectiveRestriction.StartTimeLimitation).Return(startTimeLimitation).Repeat.AtLeastOnce();
			Expect.Call(_effectiveRestriction.EndTimeLimitation).Return(endTimeLimitation).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestriction.Absence).Return(null).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			Assert.That(cellData3.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.True);
			Assert.That(cellData2.ViolatesNightlyRest, Is.True);
			Assert.That(cellData3.ViolatesNightlyRest, Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotSetViolatesIfNotTooShortNightRest()
		{
            var date1 = new DateOnly(2011, 9, 1);
            var date2 = new DateOnly(2011, 9, 2);
            var date3 = new DateOnly(2011, 9, 3);
			var endTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(20));
			var startTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(8));

			var cellData1 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			var cellData3 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date3 };
			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, cellData2 }, { 3, cellData3 } };

			Expect.Call(_effectiveRestriction.StartTimeLimitation).Return(startTimeLimitation).Repeat.AtLeastOnce();
			Expect.Call(_effectiveRestriction.EndTimeLimitation).Return(endTimeLimitation).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestriction.Absence).Return(null).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			Assert.That(cellData3.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			Assert.That(cellData3.ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldAddOneDayTooStartDateIfNotRestriction()
		{
			var date1 = new DateOnly(2011, 9, 1);
			var date2 = new DateOnly(2011, 9, 2);
			var date3 = new DateOnly(2011, 9, 3);
			var endTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(20));
			var startTimeLimitation = new StartTimeLimitation(null, null);
			var cellData1 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			var cellData3 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date3 };
			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, cellData2 }, { 3, cellData3 } };


			Expect.Call(_effectiveRestriction.StartTimeLimitation).Return(startTimeLimitation).Repeat.AtLeastOnce();
			Expect.Call(_effectiveRestriction.EndTimeLimitation).Return(endTimeLimitation).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestriction.Absence).Return(null).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			Assert.That(cellData3.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			Assert.That(cellData3.ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotCrashOnNullDay()
		{
			var date1 = new DateOnly(2011, 9, 1);
			var date2 = new DateOnly(2011, 9, 2);
			var cellData1 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, null } };

			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
		}
		[Test]
		public void ShouldNotViolateOnRealDayOffOnDay()
		{
			var date1 = new DateOnly(2011, 9, 1);
			var date2 = new DateOnly(2011, 9, 2);
			var cellData1 = new PreferenceCellData { HasDayOff = true, EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, cellData2 } };

			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
		}

		[Test]
		public void ShouldNotViolateOnDayOffOnDayOne()
		{
			var date1 = new DateOnly(2011, 9, 1);
			var date2 = new DateOnly(2011, 9, 2);
			var cellData1 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, cellData2 } };

			var dayOff = _mocks.StrictMock<IDayOffTemplate>();
			Expect.Call(_effectiveRestriction.DayOffTemplate).Return(dayOff).Repeat.AtLeastOnce();
			
			_mocks.ReplayAll();
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotViolateOnDayOffOnDayTwo()
		{
			var date1 = new DateOnly(2011, 9, 1);
			var date2 = new DateOnly(2011, 9, 2);
			var cellData1 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };

			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, cellData2 } };

			var dayOff = _mocks.StrictMock<IDayOffTemplate>();
			Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null);
			Expect.Call(_effectiveRestriction.DayOffTemplate).Return(dayOff);
			_mocks.ReplayAll();
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotViolateOnAbsenceOnDayOne()
		{
			var date1 = new DateOnly(2011, 9, 1);
			var date2 = new DateOnly(2011, 9, 2);
			var cellData1 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, cellData2 } };

			var absence = _mocks.StrictMock<IAbsence>();
			Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null).Repeat.AtLeastOnce();
			Expect.Call(_effectiveRestriction.Absence).Return(absence).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotViolateOnAbsenceOnDayTwo()
		{
			var date1 = new DateOnly(2011, 9, 1);
			var date2 = new DateOnly(2011, 9, 2);
			var cellData1 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date1 };
			var cellData2 = new PreferenceCellData { EffectiveRestriction = _effectiveRestriction, NightlyRest = TimeSpan.FromHours(12), TheDate = date2 };
			
			var cellDatas = new Dictionary<int, IPreferenceCellData> { { 1, cellData1 }, { 2, cellData2 } };

			var absence = _mocks.StrictMock<IAbsence>();
			Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null).Repeat.AtLeastOnce();
			Expect.Call(_effectiveRestriction.Absence).Return(null);
			Expect.Call(_effectiveRestriction.Absence).Return(absence);
			_mocks.ReplayAll();
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_target.CheckNightlyRest(cellDatas);
			Assert.That(cellData1.ViolatesNightlyRest, Is.False);
			Assert.That(cellData2.ViolatesNightlyRest, Is.False);
			_mocks.VerifyAll();
		}
		[Test]
		public void ShouldJustSkipIfDictionaryIsNull()
		{
			_target.CheckNightlyRest(null);
		}
	}

	
}