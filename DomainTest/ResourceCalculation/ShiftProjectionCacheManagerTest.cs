using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
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
				Expect.Call(_ruleSetProjectionEntityService.ProjectionCollection(_ruleSet, callback)).Return(GetWorkShiftsInfo()).IgnoreArguments();
                Expect.Call(_shiftFromMasterActivityService.Generate(GetWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>());
                Expect.Call(_shiftFromMasterActivityService.Generate(GetWorkShifts()[0])).IgnoreArguments().Return(GetWorkShifts());
                Expect.Call(_shiftFromMasterActivityService.Generate(GetWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>());    
            }

            using (_mocks.Playback())
            {
                var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, false);
                Assert.IsNotNull(ret);
                Assert.AreEqual(3,ret.Count);
            }
        }

        [Test]
        public void CanAdjustWorkShiftsFromRuleSetBag()
        {
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSet3 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var templateGenerator1 = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
            var templateGenerator2 = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
            var templateGenerator3 = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
            var startPeriod1 = new TimePeriodWithSegment(new TimePeriod(6, 0, 7, 0), TimeSpan.FromMinutes(15));
            var endPeriod1 = new TimePeriodWithSegment(new TimePeriod(17, 0, 19, 0), TimeSpan.FromMinutes(15));
            var startPeriod2 = new TimePeriodWithSegment(new TimePeriod(9, 0, 11, 0), TimeSpan.FromMinutes(15));
            var endPeriod2 = new TimePeriodWithSegment(new TimePeriod(16, 0, 18, 0), TimeSpan.FromMinutes(15));
            var startPeriod3 = new TimePeriodWithSegment(new TimePeriod(8, 0, 10, 0), TimeSpan.FromMinutes(15));
            var endPeriod3 = new TimePeriodWithSegment(new TimePeriod(16, 0, 16, 59), TimeSpan.FromMinutes(15));
            
            IList<IWorkShiftRuleSet> ruleSets = new List<IWorkShiftRuleSet> { ruleSet1, ruleSet2, ruleSet3 };
            var readOnlyRuleSets = new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets);
            var dateOnly = new DateOnly(2009, 2, 2);
            TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            var restriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10)),
                                      new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(19)),
                                      new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
            using (_mocks.Record())
            {
                ExpectCodeCanAdjustWorkShiftsFromRuleSetBag(startPeriod2, startPeriod1, templateGenerator3, endPeriod3,
                                                            startPeriod3, endPeriod2, templateGenerator2, ruleSet2,
                                                            ruleSet1, readOnlyRuleSets, templateGenerator1, dateOnly,
                                                            ruleSet3);
            }

            using (_mocks.Playback())
            {
                var ret = _target.ShiftProjectionCachesFromAdjustedRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, false, restriction);
                Assert.That(ret.Count, Is.EqualTo(3));
            }
        }

        private void ExpectCodeCanAdjustWorkShiftsFromRuleSetBag(TimePeriodWithSegment startPeriod2,
                                                                 TimePeriodWithSegment startPeriod1,
                                                                 IWorkShiftTemplateGenerator templateGenerator3,
                                                                 TimePeriodWithSegment endPeriod3,
                                                                 TimePeriodWithSegment startPeriod3,
                                                                 TimePeriodWithSegment endPeriod2,
                                                                 IWorkShiftTemplateGenerator templateGenerator2,
                                                                 IWorkShiftRuleSet ruleSet2, IWorkShiftRuleSet ruleSet1,
                                                                 ReadOnlyCollection<IWorkShiftRuleSet> readOnlyRuleSets,
                                                                 IWorkShiftTemplateGenerator templateGenerator1,
                                                                 DateOnly dateOnly, IWorkShiftRuleSet ruleSet3)
        {
            Expect.Call(_ruleSetBag.RuleSetCollection).Return(readOnlyRuleSets);
            Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
            Expect.Call(ruleSet2.OnlyForRestrictions).Return(false);
            Expect.Call(ruleSet3.OnlyForRestrictions).Return(false);
            Expect.Call(ruleSet1.IsValidDate(dateOnly)).Return(true);
            Expect.Call(ruleSet2.IsValidDate(dateOnly)).Return(true);
            Expect.Call(ruleSet3.IsValidDate(dateOnly)).Return(true);
            Expect.Call(ruleSet1.Clone()).Return(ruleSet1);
            Expect.Call(ruleSet2.Clone()).Return(ruleSet2);
            Expect.Call(ruleSet3.Clone()).Return(ruleSet3);
            Expect.Call(ruleSet1.TemplateGenerator).Return(templateGenerator1).Repeat.AtLeastOnce();
            Expect.Call(ruleSet2.TemplateGenerator).Return(templateGenerator2).Repeat.AtLeastOnce();
            Expect.Call(ruleSet3.TemplateGenerator).Return(templateGenerator3).Repeat.AtLeastOnce();
            Expect.Call(templateGenerator1.StartPeriod).Return(startPeriod1).Repeat.AtLeastOnce();
            Expect.Call(templateGenerator2.StartPeriod).Return(startPeriod2).Repeat.AtLeastOnce();
            Expect.Call(templateGenerator2.StartPeriod = new TimePeriodWithSegment(9, 0, 10, 0, 15));
            Expect.Call(templateGenerator2.EndPeriod).Return(endPeriod2).Repeat.AtLeastOnce();
            Expect.Call(templateGenerator2.EndPeriod = new TimePeriodWithSegment(17, 0, 18, 0, 15));
            Expect.Call(templateGenerator3.StartPeriod).Return(startPeriod3).Repeat.AtLeastOnce();
            Expect.Call(templateGenerator3.EndPeriod).Return(endPeriod3).Repeat.AtLeastOnce();
            Expect.Call(_activityChecker.ContainsDeletedActivity(ruleSet2)).Return(false);
            Expect.Call(_shiftCategoryChecker.ContainsDeletedActivity(ruleSet2)).Return(false);
            Expect.Call(_ruleSetProjectionEntityService.ProjectionCollection(ruleSet2, null)).IgnoreArguments().Return(GetWorkShiftsInfo());
            Expect.Call(_shiftFromMasterActivityService.Generate(GetWorkShifts()[0])).IgnoreArguments().Return(
                new List<IWorkShift>());
            Expect.Call(_shiftFromMasterActivityService.Generate(GetWorkShifts()[0])).IgnoreArguments().Return(GetWorkShifts());
            Expect.Call(_shiftFromMasterActivityService.Generate(GetWorkShifts()[0])).IgnoreArguments().Return(
                new List<IWorkShift>());
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
				var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, false);
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
				var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag,false);
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
                var ret = _target.ShiftProjectionCachesFromRuleSetBag(dateOnly, timeZoneInfo, _ruleSetBag, true);
                Assert.IsNotNull(ret);
                Assert.AreEqual(0, ret.Count);
            }
        }

        private IList<IWorkShift> GetWorkShifts()
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

        private IList<IWorkShiftVisualLayerInfo> GetWorkShiftsInfo()
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
