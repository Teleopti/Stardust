using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
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
		private IShiftProjectionCache _shiftProjectionCache;
		private IWorkShift _workShift;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftProjectionCacheManager = _mocks.StrictMock<IShiftProjectionCacheManager>();
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_target = new LegalShiftDecider(_shiftProjectionCacheManager, _scheduleDayEquator);
			_ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
			_shiftProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
			_workShift = _mocks.StrictMock<IWorkShift>();
		}

		[Test]
		public void ShouldReturnTrueIfShiftIsLegal()
		{
			var editableShift = new EditableShift(new ShiftCategory("hej"));
			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(new DateOnly(), TimeZoneInfo.Utc,
					_ruleSetBag, false, true)).Return(new List<IShiftProjectionCache> {_shiftProjectionCache});
				Expect.Call(_shiftProjectionCache.TheWorkShift).Return(_workShift);
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);

				Expect.Call(_shiftProjectionCache.TheWorkShift).Return(_workShift);
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(editableShift, editableShift)).Return(true);
			}
			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.IsLegalShift(new DateOnly(), TimeZoneInfo.Utc, _ruleSetBag, _shiftProjectionCache));
			}
		}

		[Test]
		public void ShouldReturnFalseIfShiftIsNotLegal()
		{
			var editableShift = new EditableShift(new ShiftCategory("hej"));
			using (_mocks.Record())
			{
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(new DateOnly(), TimeZoneInfo.Utc,
					_ruleSetBag, false, true)).Return(new List<IShiftProjectionCache> { _shiftProjectionCache });
				Expect.Call(_shiftProjectionCache.TheWorkShift).Return(_workShift);
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);

				Expect.Call(_shiftProjectionCache.TheWorkShift).Return(_workShift);
				Expect.Call(_workShift.ToEditorShift(null, TimeZoneInfo.Utc))
					.IgnoreArguments()
					.Return(editableShift);
				Expect.Call(_scheduleDayEquator.MainShiftEquals(editableShift, editableShift)).Return(false);
			}
			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.IsLegalShift(new DateOnly(), TimeZoneInfo.Utc, _ruleSetBag, _shiftProjectionCache));
			}
		}
	}
}