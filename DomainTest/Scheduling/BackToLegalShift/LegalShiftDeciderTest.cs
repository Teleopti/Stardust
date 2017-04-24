using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.BackToLegalShift
{
	[TestFixture]
	public class LegalShiftDeciderTest
	{
		private MockRepository _mocks;
		private ILegalShiftDecider _target;
		private IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private IScheduleDayEquator _scheduleDayEquator;
		private IRuleSetBag _ruleSetBag;
		private ShiftProjectionCache _shiftProjectionCache;
		private IWorkShift _workShift;
		private IScheduleDay _scheduleDay;
		private IPersonAssignment _personAssignment;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftProjectionCacheManager = _mocks.StrictMock<IShiftProjectionCacheManager>();
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_target = new LegalShiftDecider(_shiftProjectionCacheManager, _scheduleDayEquator);
			_ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
			_workShift = _mocks.StrictMock<IWorkShift>();
			_shiftProjectionCache = new ShiftProjectionCache(_workShift, new PersonalShiftMeetingTimeChecker());
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_personAssignment = new PersonAssignment(new Person(), new Scenario("_"), new DateOnly());
		}

		[Test]
		public void ShouldReturnTrueIfShiftIsLegal()
		{
			var editableShift = new EditableShift(new ShiftCategory("hej"));
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(), TimeZoneInfo.Utc);
			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsDateTimePeriod,_ruleSetBag, false, true))
					.Return(new List<ShiftProjectionCache> {_shiftProjectionCache});
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);
				
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(editableShift, editableShift)).Return(true);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(_personAssignment);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersonMeetingCollection())
					.Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.IsLegalShift(_ruleSetBag, _shiftProjectionCache, _scheduleDay));
			}
		}

		[Test]
		public void ShouldReturnFalseIfShiftIsNotLegal()
		{
			var editableShift = new EditableShift(new ShiftCategory("hej"));
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(), TimeZoneInfo.Utc);
			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(dateOnlyAsDateTimePeriod, _ruleSetBag,
					false, true)).Return(new List<ShiftProjectionCache> {_shiftProjectionCache});
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);
				
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(editableShift, editableShift)).Return(false);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(_personAssignment);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersonMeetingCollection())
					.Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.IsLegalShift(_ruleSetBag, _shiftProjectionCache, _scheduleDay));
			}
		}
	}
}