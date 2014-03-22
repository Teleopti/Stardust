using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ShiftProjectionCacheManagerTest
    {
        private IShiftProjectionCacheManager _target;
        private MockRepository _mocks;
        private IRuleSetBag _ruleSetBag;
        private IWorkShiftRuleSet _ruleSet;
        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IShiftCategory _category;
        private IActivity _activity;
        private IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
        private IShiftFromMasterActivityService _shiftFromMasterActivityService;
        private IRuleSetDeletedActivityChecker _activityChecker;
    	private IRuleSetDeletedShiftCategoryChecker _shiftCategoryChecker;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _ruleSetProjectionEntityService = _mocks.StrictMock<IRuleSetProjectionEntityService>();
            _shiftFromMasterActivityService = _mocks.StrictMock<IShiftFromMasterActivityService>();
            _activityChecker = _mocks.StrictMock<IRuleSetDeletedActivityChecker>();
        	_shiftCategoryChecker = _mocks.StrictMock<IRuleSetDeletedShiftCategoryChecker>();
            _target = new ShiftProjectionCacheManager(_shiftFromMasterActivityService, _activityChecker, _shiftCategoryChecker, _ruleSetProjectionEntityService);
            _ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            _ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
            
        }

        [Test]
        public void CanGetWorkShiftsFromRuleSetBag()
        {
            IList<IWorkShiftRuleSet> ruleSets = new List<IWorkShiftRuleSet> { _ruleSet };
            var readOnlyRuleSets = new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets);
            var dateOnly = new DateOnly(2009, 2, 2);
            TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
	        var callback = _mocks.DynamicMock<IWorkShiftAddCallback>();
            using (_mocks.Record())
            {
                Expect.Call(_ruleSetBag.RuleSetCollection).Return(readOnlyRuleSets);
                Expect.Call(_ruleSet.OnlyForRestrictions).Return(false);
                Expect.Call(_ruleSet.IsValidDate(dateOnly)).Return(true);
                Expect.Call(_activityChecker.ContainsDeletedActivity(_ruleSet)).Return(false);
				Expect.Call(_shiftCategoryChecker.ContainsDeletedActivity(_ruleSet)).Return(false);
				Expect.Call(_ruleSetProjectionEntityService.ProjectionCollection(_ruleSet, callback)).Return(getWorkShiftsInfo()).IgnoreArguments();
                Expect.Call(_shiftFromMasterActivityService.Generate(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>());
                Expect.Call(_shiftFromMasterActivityService.Generate(getWorkShifts()[0])).IgnoreArguments().Return(getWorkShifts());
                Expect.Call(_shiftFromMasterActivityService.Generate(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>());    
            }

            using (_mocks.Playback())
            {
                var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, false, true);
                Assert.IsNotNull(ret);
                Assert.AreEqual(3,ret.Count);
            }
        }

        [Test]
		public void ShouldNotGetAnyWorkShiftsWhenDeletedShiftCategoryInRuleSetBag()
		{
			var ruleSets = new List<IWorkShiftRuleSet> { _ruleSet };
			var readOnlyRuleSets = new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets);
			var dateOnly = new DateOnly(2009, 2, 2);
			var timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
			using (_mocks.Record())
			{
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(readOnlyRuleSets);
                Expect.Call(_ruleSet.OnlyForRestrictions).Return(false);
				Expect.Call(_ruleSet.IsValidDate(dateOnly)).Return(true);
				Expect.Call(_activityChecker.ContainsDeletedActivity(_ruleSet)).Return(false);
				Expect.Call(_shiftCategoryChecker.ContainsDeletedActivity(_ruleSet)).Return(true);
			}

			using (_mocks.Playback())
			{
				var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, false, true);
				Assert.IsNotNull(ret);
				Assert.AreEqual(0, ret.Count);
			}	
		}

		[Test]
		public void ShouldNotGetAnyWorkShiftsWhenDeletedActivityInRuleSetBag()
		{
			var ruleSets = new List<IWorkShiftRuleSet> { _ruleSet };
			var readOnlyRuleSets = new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets);
			var dateOnly = new DateOnly(2009, 2, 2);
			var timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
			using (_mocks.Record())
			{
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(readOnlyRuleSets);
                Expect.Call(_ruleSet.OnlyForRestrictions).Return(false);
				Expect.Call(_ruleSet.IsValidDate(dateOnly)).Return(true);
				Expect.Call(_activityChecker.ContainsDeletedActivity(_ruleSet)).Return(true);
			}

			using (_mocks.Playback())
			{
				var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, false, true);
				Assert.IsNotNull(ret);
				Assert.AreEqual(0, ret.Count);
			}		
		}

        [Test]
        public void ShouldCheckIfOnlyForRestrictionsWhenReturningWorkShifts()
        {
            _activity = ActivityFactory.CreateActivity("sd");
            _category = ShiftCategoryFactory.CreateShiftCategory("dv");
            var workShift = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
                                                                      _activity, _category);
            IWorkShiftVisualLayerInfo info1 = new WorkShiftVisualLayerInfo(workShift, null);
            var infos = new List<IWorkShiftVisualLayerInfo> { info1 };
            var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSets = new List<IWorkShiftRuleSet> { _ruleSet, ruleSet2 };
            var readOnlyRuleSets = new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets);
            var dateOnly = new DateOnly(2009, 2, 2);
            var timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
			var callback = _mocks.DynamicMock<IWorkShiftAddCallback>();
            using (_mocks.Record())
            {
                Expect.Call(_ruleSetBag.RuleSetCollection).Return(readOnlyRuleSets);
                Expect.Call(_ruleSet.IsValidDate(dateOnly)).Return(true);
                Expect.Call(_ruleSet.OnlyForRestrictions).Return(true);
                Expect.Call(ruleSet2.OnlyForRestrictions).Return(false);
                Expect.Call(_activityChecker.ContainsDeletedActivity(_ruleSet)).Return(false);
                Expect.Call(_shiftCategoryChecker.ContainsDeletedActivity(_ruleSet)).Return(false);
				Expect.Call(_ruleSetProjectionEntityService.ProjectionCollection(_ruleSet, callback)).Return(infos).IgnoreArguments();
                Expect.Call(_shiftFromMasterActivityService.Generate(workShift)).Return(new List<IWorkShift>());
            }

            using (_mocks.Playback())
            {
                var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, true, true);
                Assert.IsNotNull(ret);
                Assert.AreEqual(0, ret.Count);
            }
        }

	    [Test]
	    public void ShouldNotCheckIsValidDateIfNotAsked()
	    {
			IList<IWorkShiftRuleSet> ruleSets = new List<IWorkShiftRuleSet> { _ruleSet };
			var readOnlyRuleSets = new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets);
			var dateOnly = new DateOnly(2009, 2, 2);
			TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
			var callback = _mocks.DynamicMock<IWorkShiftAddCallback>();
			using (_mocks.Record())
			{
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(readOnlyRuleSets);
				Expect.Call(_ruleSet.OnlyForRestrictions).Return(false);
				Expect.Call(_activityChecker.ContainsDeletedActivity(_ruleSet)).Return(false);
				Expect.Call(_shiftCategoryChecker.ContainsDeletedActivity(_ruleSet)).Return(false);
				Expect.Call(_ruleSetProjectionEntityService.ProjectionCollection(_ruleSet, callback)).Return(getWorkShiftsInfo()).IgnoreArguments();
				Expect.Call(_shiftFromMasterActivityService.Generate(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>());
				Expect.Call(_shiftFromMasterActivityService.Generate(getWorkShifts()[0])).IgnoreArguments().Return(getWorkShifts());
				Expect.Call(_shiftFromMasterActivityService.Generate(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>());
			}

			using (_mocks.Playback())
			{
				var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, false, false);
				Assert.IsNotNull(ret);
				Assert.AreEqual(3, ret.Count);
			}
	    }

        private IList<IWorkShift> getWorkShifts()
        {
            _activity = ActivityFactory.CreateActivity("sd");
            _category = ShiftCategoryFactory.CreateShiftCategory("dv");
            _workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
                                                           _activity, _category);
            _workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
                                                           _activity, _category);
            _workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
                                                           _activity, _category);

            return new List<IWorkShift> {_workShift1, _workShift2, _workShift3};
        }

        private IList<IWorkShiftVisualLayerInfo> getWorkShiftsInfo()
        {
            _activity = ActivityFactory.CreateActivity("sd");
            _category = ShiftCategoryFactory.CreateShiftCategory("dv");
            _workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
                                                          _activity, _category);
            _workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
                                                          _activity, _category);
            _workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
                                                                      _activity, _category);

            IWorkShiftVisualLayerInfo info1 = new WorkShiftVisualLayerInfo(_workShift1, null);
            IWorkShiftVisualLayerInfo info2 = new WorkShiftVisualLayerInfo(_workShift2, null);
            IWorkShiftVisualLayerInfo info3 = new WorkShiftVisualLayerInfo(_workShift3, null);
            return new List<IWorkShiftVisualLayerInfo> { info1, info2, info3 };
        }    
    }   
}
