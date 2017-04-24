using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ShiftProjectionCacheManagerTest
    {
        private ShiftProjectionCacheManager _target;
        private IRuleSetBag _ruleSetBag;
        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IShiftCategory _category;
        private IActivity _activity;
        private IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
        private IShiftFromMasterActivityService _shiftFromMasterActivityService;
        private IRuleSetDeletedActivityChecker _activityChecker;
    	private IRuleSetDeletedShiftCategoryChecker _shiftCategoryChecker;
	    private IWorkShiftFromEditableShift _workShiftFromEditableShift;
		private readonly TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
		private readonly DateOnly dateOnly = new DateOnly(2009, 2, 2);

	    [SetUp]
        public void Setup()
        {
            _ruleSetProjectionEntityService = MockRepository.GenerateMock<IRuleSetProjectionEntityService>();
			_shiftFromMasterActivityService = MockRepository.GenerateMock<IShiftFromMasterActivityService>();
			_activityChecker = MockRepository.GenerateMock<IRuleSetDeletedActivityChecker>();
			_shiftCategoryChecker = MockRepository.GenerateMock<IRuleSetDeletedShiftCategoryChecker>();
			_workShiftFromEditableShift = MockRepository.GenerateMock<IWorkShiftFromEditableShift>();
            _target = new ShiftProjectionCacheManager(_shiftFromMasterActivityService, _activityChecker, _shiftCategoryChecker, _ruleSetProjectionEntityService, _workShiftFromEditableShift);
			_ruleSetBag = new RuleSetBag();
			_activity = ActivityFactory.CreateActivity("sd");
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
        }

	    [Test]
	    public void CanGetWorkShiftsFromRuleSets()
	    {
			var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category));

			var callback = MockRepository.GenerateMock<IWorkShiftAddCallback>();

			_activityChecker.Stub(x => x.ContainsDeletedActivity(workShiftRuleSet)).Return(false);
			_shiftCategoryChecker.Stub(x => x.ContainsDeletedShiftCategory(workShiftRuleSet)).Return(false);
			_ruleSetProjectionEntityService.Stub(x => x.ProjectionCollection(workShiftRuleSet, callback)).Return(getWorkShiftsInfo()).IgnoreArguments();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>()).Repeat.Once();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(getWorkShifts()).Repeat.Once();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>()).Repeat.Once();

			var ret = _target.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(dateOnly, timeZoneInfo), new [] {workShiftRuleSet}, false, true);
			Assert.IsNotNull(ret);
			Assert.AreEqual(3, ret.Count);
		}

	    [Test]
	    public void CanGetWorkShiftsFromRuleSetBag()
	    {
		    var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category));
		    
		    _ruleSetBag.AddRuleSet(workShiftRuleSet);
		    var callback = MockRepository.GenerateMock<IWorkShiftAddCallback>();

		    _activityChecker.Stub(x => x.ContainsDeletedActivity(workShiftRuleSet)).Return(false);
		    _shiftCategoryChecker.Stub(x => x.ContainsDeletedShiftCategory(workShiftRuleSet)).Return(false);
		    _ruleSetProjectionEntityService.Stub(x => x.ProjectionCollection(workShiftRuleSet, callback)).Return(getWorkShiftsInfo()).IgnoreArguments();
		    _shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>()).Repeat.Once();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(getWorkShifts()).Repeat.Once();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>()).Repeat.Once();

		    var ret = _target.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(dateOnly, timeZoneInfo), _ruleSetBag, false, true);
		    Assert.IsNotNull(ret);
		    Assert.AreEqual(3, ret.Count);
	    }

		[Test]
		public void ShouldClearCacheManagerOnDispose()
		{
			var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category));

			_ruleSetBag.AddRuleSet(workShiftRuleSet);
			var callback = MockRepository.GenerateMock<IWorkShiftAddCallback>();

			_activityChecker.Stub(x => x.ContainsDeletedActivity(workShiftRuleSet)).Return(false);
			_shiftCategoryChecker.Stub(x => x.ContainsDeletedShiftCategory(workShiftRuleSet)).Return(false);
			_ruleSetProjectionEntityService.Stub(x => x.ProjectionCollection(workShiftRuleSet, callback)).Return(getWorkShiftsInfo()).IgnoreArguments();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>());

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, timeZoneInfo);
			_target.ShiftProjectionCachesFromRuleSets(dateOnlyAsDateTimePeriod, _ruleSetBag, false, true);
			_target.Dispose();
			_target.ShiftProjectionCachesFromRuleSets(dateOnlyAsDateTimePeriod, _ruleSetBag, false, true);

			_ruleSetProjectionEntityService.AssertWasCalled(x => x.ProjectionCollection(workShiftRuleSet, callback), o => o.IgnoreArguments().Repeat.Twice());
		}

	    [Test]
	    public void ShouldNotGetAnyWorkShiftsWhenDeletedShiftCategoryInRuleSetBag()
	    {
		    var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category));

		    _ruleSetBag.AddRuleSet(workShiftRuleSet);
		    var callback = MockRepository.GenerateMock<IWorkShiftAddCallback>();

		    _activityChecker.Stub(x => x.ContainsDeletedActivity(workShiftRuleSet)).Return(false);
		    _shiftCategoryChecker.Stub(x => x.ContainsDeletedShiftCategory(workShiftRuleSet)).Return(true);

		    var ret = _target.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(dateOnly, timeZoneInfo), _ruleSetBag, false, true);
		    Assert.IsNotNull(ret);
		    Assert.AreEqual(0, ret.Count);

		    _ruleSetProjectionEntityService.AssertWasNotCalled(x => x.ProjectionCollection(workShiftRuleSet, callback),o => o.IgnoreArguments());
	    }

	    [Test]
	    public void ShouldNotGetAnyWorkShiftsWhenDeletedActivityInRuleSetBag()
	    {
		    var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category));

		    _ruleSetBag.AddRuleSet(workShiftRuleSet);
		    var callback = MockRepository.GenerateMock<IWorkShiftAddCallback>();

		    _activityChecker.Stub(x => x.ContainsDeletedActivity(workShiftRuleSet)).Return(true);

		    var ret = _target.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(dateOnly, timeZoneInfo), _ruleSetBag, false, true);
		    Assert.IsNotNull(ret);
		    Assert.AreEqual(0, ret.Count);

		    _ruleSetProjectionEntityService.AssertWasNotCalled(x => x.ProjectionCollection(workShiftRuleSet, callback), o => o.IgnoreArguments());
	    }

		[Test]
		public void ShouldCheckIfOnlyForRestrictionsWhenReturningWorkShifts()
		{
		    var validWorkShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category));
		    var invalidWorkShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category)){OnlyForRestrictions = true};

		    _ruleSetBag.AddRuleSet(validWorkShiftRuleSet);
		    _ruleSetBag.AddRuleSet(invalidWorkShiftRuleSet);

			var workShift = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
				_activity, _category);
			WorkShiftVisualLayerInfo info1 = new WorkShiftVisualLayerInfo(workShift, null);
			var infos = new List<WorkShiftVisualLayerInfo> {info1};
			var callback = MockRepository.GenerateMock<IWorkShiftAddCallback>();

			_activityChecker.Stub(x => x.ContainsDeletedActivity(validWorkShiftRuleSet)).Return(false);
			_shiftCategoryChecker.Stub(x => x.ContainsDeletedShiftCategory(validWorkShiftRuleSet)).Return(false);
			_ruleSetProjectionEntityService.Stub(x => x.ProjectionCollection(validWorkShiftRuleSet, callback)).Return(infos).IgnoreArguments();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(workShift)).Return(new List<IWorkShift>());

			var ret = _target.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(dateOnly, timeZoneInfo), _ruleSetBag, true, true);
			Assert.IsNotNull(ret);
			Assert.AreEqual(0, ret.Count);
		}

		[Test]
		public void ShouldNotCheckIsValidDateIfNotAsked()
		{
			var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(), new TimePeriodWithSegment(), _category));

			_ruleSetBag.AddRuleSet(workShiftRuleSet);
			var callback = MockRepository.GenerateMock<IWorkShiftAddCallback>();

			_activityChecker.Stub(x => x.ContainsDeletedActivity(workShiftRuleSet)).Return(false);
			_shiftCategoryChecker.Stub(x => x.ContainsDeletedShiftCategory(workShiftRuleSet)).Return(false);
			_ruleSetProjectionEntityService.Stub(x => x.ProjectionCollection(workShiftRuleSet, callback)).Return(getWorkShiftsInfo()).IgnoreArguments();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>()).Repeat.Once();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(getWorkShifts()).Repeat.Once();
			_shiftFromMasterActivityService.Stub(x => x.ExpandWorkShiftsWithMasterActivity(getWorkShifts()[0])).IgnoreArguments().Return(new List<IWorkShift>()).Repeat.Once();

			var ret = _target.ShiftProjectionCachesFromRuleSets(new DateOnlyAsDateTimePeriod(dateOnly, timeZoneInfo), _ruleSetBag, false, false);
			Assert.IsNotNull(ret);
			Assert.AreEqual(3, ret.Count);
		}

	    private IList<IWorkShift> getWorkShifts()
        {
            _workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
                                                           _activity, _category);
            _workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
                                                           _activity, _category);
            _workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
                                                           _activity, _category);

            return new List<IWorkShift> {_workShift1, _workShift2, _workShift3};
        }

        private IList<WorkShiftVisualLayerInfo> getWorkShiftsInfo()
        {
            _workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
                                                          _activity, _category);
            _workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
                                                          _activity, _category);
            _workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
                                                                      _activity, _category);

            WorkShiftVisualLayerInfo info1 = new WorkShiftVisualLayerInfo(_workShift1, null);
            WorkShiftVisualLayerInfo info2 = new WorkShiftVisualLayerInfo(_workShift2, null);
            WorkShiftVisualLayerInfo info3 = new WorkShiftVisualLayerInfo(_workShift3, null);
            return new List<WorkShiftVisualLayerInfo> { info1, info2, info3 };
        }    
    }   
}
