using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class RestrictionOverLimitDeciderTest
	{
		private RestrictionOverLimitDecider _restrictionOverLimitDecider;
		private ICheckerRestriction _checkerRestriction;
		private MockRepository _mock;
		private Percent _limit;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDayPro _scheduleDayPro3;
		private IScheduleDayPro _scheduleDayPro4;
		private IScheduleDayPro _scheduleDayPro5;
		private IScheduleDayPro _scheduleDayPro6;
		private IScheduleDayPro _scheduleDayPro7;
		private IScheduleDayPro _scheduleDayPro8;
		private IScheduleDayPro _scheduleDayPro9;
		private IScheduleDayPro _scheduleDayPro10;
		private IScheduleDayPro[] _effectivePeriodDays;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IScheduleDay _scheduleDay3;
		private IScheduleDay _scheduleDay4;
		private IScheduleDay _scheduleDay5;
		private IScheduleDay _scheduleDay6;
		private IScheduleDay _scheduleDay7;
		private IScheduleDay _scheduleDay8;
		private IScheduleDay _scheduleDay9;
		private IScheduleDay _scheduleDay10;
		private DateOnly _dateOnly1;
		private DateOnly _dateOnly2;
		private DateOnly _dateOnly3;
		private DateOnly _dateOnly4;
		private DateOnly _dateOnly5;
		private DateOnly _dateOnly6;
		private DateOnly _dateOnly7;
		private DateOnly _dateOnly8;
		private DateOnly _dateOnly9;
		private DateOnly _dateOnly10;

			
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_checkerRestriction = _mock.StrictMock<ICheckerRestriction>();
			_restrictionOverLimitDecider = new RestrictionOverLimitDecider(_checkerRestriction);
			_limit = new Percent(0.8d);
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro4 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro5 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro6 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro7 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro8 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro9 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro10 = _mock.StrictMock<IScheduleDayPro>();

			_effectivePeriodDays = new [] { _scheduleDayPro1, _scheduleDayPro2, _scheduleDayPro3, _scheduleDayPro4, _scheduleDayPro5, _scheduleDayPro6, _scheduleDayPro7, _scheduleDayPro8, _scheduleDayPro9, _scheduleDayPro10 };
			
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay3 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay4 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay5 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay6 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay7 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay8 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay9 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay10 = _mock.StrictMock<IScheduleDay>();

			_dateOnly1 = new DateOnly(2013, 1, 1);
			_dateOnly2 = new DateOnly(2013, 1, 2);
			_dateOnly3 = new DateOnly(2013, 1, 3);
			_dateOnly4 = new DateOnly(2013, 1, 4);
			_dateOnly5 = new DateOnly(2013, 1, 5);
			_dateOnly6 = new DateOnly(2013, 1, 6);
			_dateOnly7 = new DateOnly(2013, 1, 7);
			_dateOnly8 = new DateOnly(2013, 1, 8);
			_dateOnly9 = new DateOnly(2013, 1, 9);
			_dateOnly10 = new DateOnly(2013, 1, 10);
		}

		[Test]
		public void ShouldDecideOnPreferencesNotOverLimit()
		{
			_limit = new Percent(0.8d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay3)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay4)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay5)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay6)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay7)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay8)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.PreferencesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(0, brokenRestrictionInfo.BrokenDays.Count);
				Assert.AreEqual(new Percent(), brokenRestrictionInfo.BrokenPercentage);
			}
		}

		[Test]
		public void ShouldDecideOnMustHavesNotOverLimit()
		{
			_limit = new Percent(0.9d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay3)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay4)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay5)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay6)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay7)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay8)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay9)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.MustHavesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(0, brokenRestrictionInfo.BrokenDays.Count);
				Assert.AreEqual(new Percent(), brokenRestrictionInfo.BrokenPercentage);
			}	
		}

		[Test]
		public void ShouldDecideOnRotationNotOverLimit()
		{
			_limit = new Percent(0.5d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay3)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay4)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay5)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay6)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay7)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay8)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro6.Day).Return(_dateOnly6);
				Expect.Call(_scheduleDayPro7.Day).Return(_dateOnly7);
				Expect.Call(_scheduleDayPro8.Day).Return(_dateOnly8);
				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.RotationOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(0, brokenRestrictionInfo.BrokenDays.Count);
				Assert.AreEqual(new Percent(), brokenRestrictionInfo.BrokenPercentage);
			}		
		}

		[Test]
		public void ShouldDecideOnAvailabilitiesNotOverLimit()
		{
			_limit = new Percent(0.3d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay3)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay4)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay5)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay6)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay7)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay8)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro4.Day).Return(_dateOnly4);
				Expect.Call(_scheduleDayPro5.Day).Return(_dateOnly5);
				Expect.Call(_scheduleDayPro6.Day).Return(_dateOnly6);
				Expect.Call(_scheduleDayPro7.Day).Return(_dateOnly7);
				Expect.Call(_scheduleDayPro8.Day).Return(_dateOnly8);
				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.AvailabilitiesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(0, brokenRestrictionInfo.BrokenDays.Count);
				Assert.AreEqual(new Percent(), brokenRestrictionInfo.BrokenPercentage);
			}			
		}

		[Test]
		public void ShouldDecideOnStudentAvailabilitiesNotOverLimit()
		{
			_limit = new Percent(0.1d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay2)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay3)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay4)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay5)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay6)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay7)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay8)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro2.Day).Return(_dateOnly2);
				Expect.Call(_scheduleDayPro3.Day).Return(_dateOnly3);
				Expect.Call(_scheduleDayPro4.Day).Return(_dateOnly4);
				Expect.Call(_scheduleDayPro5.Day).Return(_dateOnly5);
				Expect.Call(_scheduleDayPro6.Day).Return(_dateOnly6);
				Expect.Call(_scheduleDayPro7.Day).Return(_dateOnly7);
				Expect.Call(_scheduleDayPro8.Day).Return(_dateOnly8);
				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(0, brokenRestrictionInfo.BrokenDays.Count);
				Assert.AreEqual(new Percent(), brokenRestrictionInfo.BrokenPercentage);
			}	
		}

		[Test]
		public void ShouldDecideOnPreferencesOverLimit()
		{
			_limit = new Percent(0.8d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay3)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay4)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay5)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay6)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay7)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay8)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckPreference(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro8.Day).Return(_dateOnly8);
				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.PreferencesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(3, brokenRestrictionInfo.BrokenDays.Count);
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly8));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly9));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly10));
				Assert.AreEqual(new Percent(0.3d), brokenRestrictionInfo.BrokenPercentage);
			}	
		}

		[Test]
		public void ShouldDecideOnMustHavesOverLimit()
		{
			_limit = new Percent(1.0d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay1)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay3)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay4)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay5)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay6)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay7)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay8)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay9)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckPreferenceMustHave(_scheduleDay10)).Return(PermissionState.Satisfied);

				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly1);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.MustHavesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(1, brokenRestrictionInfo.BrokenDays.Count);
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly1));
				Assert.AreEqual(new Percent(0.1d), brokenRestrictionInfo.BrokenPercentage);
			}		
		}

		[Test]
		public void ShouldDecideOnRotationOverLimit()
		{
			_limit = new Percent(0.5d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay3)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay4)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay5)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay6)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay7)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay8)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckRotations(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro5.Day).Return(_dateOnly5);
				Expect.Call(_scheduleDayPro6.Day).Return(_dateOnly6);
				Expect.Call(_scheduleDayPro7.Day).Return(_dateOnly7);
				Expect.Call(_scheduleDayPro8.Day).Return(_dateOnly8);
				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.RotationOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(6, brokenRestrictionInfo.BrokenDays.Count);
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly5));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly6));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly7));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly8));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly9));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly10));
				Assert.AreEqual(new Percent(0.6d), brokenRestrictionInfo.BrokenPercentage);
			}	
		}

		[Test]
		public void ShouldDecideOnAvailabilitiesOverLimit()
		{
			_limit = new Percent(0.3d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay1)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay2)).Return(PermissionState.Satisfied);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay3)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay4)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay5)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay6)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay7)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay8)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckAvailability(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro3.Day).Return(_dateOnly3);
				Expect.Call(_scheduleDayPro4.Day).Return(_dateOnly4);
				Expect.Call(_scheduleDayPro5.Day).Return(_dateOnly5);
				Expect.Call(_scheduleDayPro6.Day).Return(_dateOnly6);
				Expect.Call(_scheduleDayPro7.Day).Return(_dateOnly7);
				Expect.Call(_scheduleDayPro8.Day).Return(_dateOnly8);
				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.AvailabilitiesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(8, brokenRestrictionInfo.BrokenDays.Count);
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly3));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly4));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly5));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly6));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly7));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly8));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly9));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly10));
				Assert.AreEqual(new Percent(0.8d), brokenRestrictionInfo.BrokenPercentage);
			}					
		}

		[Test]
		public void ShouldDecideOnStudentAvailabilityOverLimit()
		{
			_limit = new Percent(0.1d);

			using (_mock.Record())
			{
				commonExpects();

				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay1)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay2)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay3)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay4)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay5)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay6)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay7)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay8)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay9)).Return(PermissionState.Broken);
				Expect.Call(_checkerRestriction.CheckStudentAvailability(_scheduleDay10)).Return(PermissionState.Broken);

				Expect.Call(_scheduleDayPro1.Day).Return(_dateOnly1);
				Expect.Call(_scheduleDayPro2.Day).Return(_dateOnly2);
				Expect.Call(_scheduleDayPro3.Day).Return(_dateOnly3);
				Expect.Call(_scheduleDayPro4.Day).Return(_dateOnly4);
				Expect.Call(_scheduleDayPro5.Day).Return(_dateOnly5);
				Expect.Call(_scheduleDayPro6.Day).Return(_dateOnly6);
				Expect.Call(_scheduleDayPro7.Day).Return(_dateOnly7);
				Expect.Call(_scheduleDayPro8.Day).Return(_dateOnly8);
				Expect.Call(_scheduleDayPro9.Day).Return(_dateOnly9);
				Expect.Call(_scheduleDayPro10.Day).Return(_dateOnly10);

			}

			using (_mock.Playback())
			{
				var brokenRestrictionInfo = _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(_limit, _scheduleMatrixPro);
				Assert.AreEqual(10, brokenRestrictionInfo.BrokenDays.Count);
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly1));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly2));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly3));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly4));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly5));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly6));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly7));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly8));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly9));
				Assert.IsTrue(brokenRestrictionInfo.BrokenDays.Contains(_dateOnly10));
				Assert.AreEqual(new Percent(1d), brokenRestrictionInfo.BrokenPercentage);
			}		
		}

		private void commonExpects()
		{
			Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(_effectivePeriodDays);
			Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
			Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
			Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay3);
			Expect.Call(_scheduleDayPro4.DaySchedulePart()).Return(_scheduleDay4);
			Expect.Call(_scheduleDayPro5.DaySchedulePart()).Return(_scheduleDay5);
			Expect.Call(_scheduleDayPro6.DaySchedulePart()).Return(_scheduleDay6);
			Expect.Call(_scheduleDayPro7.DaySchedulePart()).Return(_scheduleDay7);
			Expect.Call(_scheduleDayPro8.DaySchedulePart()).Return(_scheduleDay8);
			Expect.Call(_scheduleDayPro9.DaySchedulePart()).Return(_scheduleDay9);
			Expect.Call(_scheduleDayPro10.DaySchedulePart()).Return(_scheduleDay10);	
		}
	}
}
