using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class VisualizePresenterTest
    {
        private IVisualizePresenter _target;
        private MockRepository _mock;
        private IDataHelper _dataHelper;
        private IExplorerViewModel _model;
        private IExplorerPresenter _explorerPresenter;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IRuleSetBag _newRuleSetBag;
        private readonly TypedBindingCollection<IActivity> activities = new TypedBindingCollection<IActivity>();
        private readonly TypedBindingCollection<IShiftCategory> categories = new TypedBindingCollection<IShiftCategory>();
        private IShiftCreatorService _shiftCreatorService;
        private IWorkShiftRuleSet _workShiftRuleSet;
        private IActivity _activity;
        private IShiftCategory _category;
	    private IExplorerView _explorerView;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dataHelper = _mock.StrictMock<IDataHelper>();
            _explorerPresenter = _mock.StrictMock<IExplorerPresenter>();

            _model = new ExplorerViewModel { DefaultSegment = 15 };

            _activity = ActivityFactory.CreateActivity("test");
            _category = ShiftCategoryFactory.CreateShiftCategory("CategoryC");

            activities.Add(_activity);
            categories.Add(_category);
            
            _newRuleSetBag = new RuleSetBag();

            _ruleSetBagCollection = new List<IRuleSetBag>();
			_shiftCreatorService = _mock.StrictMock<IShiftCreatorService>();
        	_ruleSetBagCollection.Add(_newRuleSetBag);

            _workShiftRuleSet = new WorkShiftRuleSet(GetTemplateGenerator());
            _ruleSetCollection = new List<IWorkShiftRuleSet> {_workShiftRuleSet};

		    _model.SetActivityCollection(activities);
            _model.SetCategoryCollection(categories);

            _model.SetRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            _model.SetRuleSetBagCollection(new ReadOnlyCollection<IRuleSetBag>(_ruleSetBagCollection));
            _model.SetFilteredRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));

        	_target = new VisualizePresenter(_explorerPresenter, _dataHelper,
        	                                 new RuleSetProjectionEntityService(_shiftCreatorService));

    		_explorerView = _mock.DynamicMock<IExplorerView>();
        }

        [Test]
        public void VerifyLoadModelCollection()
        {
			var callback = new WorkShiftAddCallback();
            using (_mock.Record())
            {
				Expect.Call(_explorerPresenter.View).Return(_explorerView).Repeat.Twice();
                Expect.Call(_explorerPresenter.Model).Return(_model);
				Expect.Call(_shiftCreatorService.Generate(_workShiftRuleSet, null)).Return(CreateWorkShiftList()).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection(callback);
                _target.RuleSetAmounts();
                _target.ContractTimes();
                Assert.IsNotNull(_target.ModelCollection);
                Assert.IsNotNull(_target.RuleSetAmounts());
                Assert.IsNotNull(_target.ContractTimes());
                Assert.AreEqual(2, _target.ModelCollection.Count);
                Assert.AreEqual(2, _target.GetNumberOfRowsToBeShown());
            }
        }

        [Test]
        public void VerifyGetNumberOfRowsToBeShown()
        {
			var callback = new WorkShiftAddCallback();
            using (_mock.Record())
            {
				Expect.Call(_explorerPresenter.View).Return(_explorerView).Repeat.Twice();
                Expect.Call(_explorerPresenter.Model).Return(_model);
				Expect.Call(_shiftCreatorService.Generate(_workShiftRuleSet,null)).Return(CreateWorkShiftList()).IgnoreArguments();
            }
            using (_mock.Playback())
            {
				_target.LoadModelCollection(callback);
                Assert.IsNotNull(_target.ModelCollection);
                Assert.AreEqual(2, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyCopyWorkShiftToSessionDataClip()
        {
			var callback = new WorkShiftAddCallback();
        	using (_mock.Record())
            {
				Expect.Call(_explorerPresenter.View).Return(_explorerView).Repeat.Twice();
                Expect.Call(_explorerPresenter.Model).Return(_model).Repeat.Times(2);
                Expect.Call(_shiftCreatorService.Generate(_workShiftRuleSet, null)).Return(CreateWorkShiftList()).Repeat.Twice().IgnoreArguments();
            }
            using (_mock.Playback())
            {
				ShiftInClip.Data = null;
				_target.LoadModelCollection(callback);
                _target.CopyWorkShiftToSessionDataClip(1);
                IWorkShift result = ShiftInClip.Data;
                Assert.IsNotNull(result);
            }
        }

        [Test]
        public void VerifyCopyWorkShiftToSessionDataClipWhenRowIndexIsLessThanOne()
        {
			var callback = new WorkShiftAddCallback();
            using (_mock.Record())
            {
				Expect.Call(_explorerPresenter.View).Return(_explorerView).Repeat.Twice();
                Expect.Call(_explorerPresenter.Model).Return(_model);
				Expect.Call(_shiftCreatorService.Generate(_workShiftRuleSet,null)).Return(CreateWorkShiftList()).IgnoreArguments();
            }
            using (_mock.Playback())
            {
				ShiftInClip.Data = null;
				_target.LoadModelCollection(callback);
                _target.CopyWorkShiftToSessionDataClip(0);
                IWorkShift result = ShiftInClip.Data;
                Assert.IsNull(result);
            }
        }

        private IWorkShiftTemplateGenerator GetTemplateGenerator()
        {
            TimePeriodWithSegment tp1 = new TimePeriodWithSegment(new TimePeriod(8, 0, 9, 0), TimeSpan.FromMinutes(15));
            TimePeriodWithSegment tp2 = new TimePeriodWithSegment(new TimePeriod(17, 0, 18, 0), TimeSpan.FromMinutes(15));

            return new WorkShiftTemplateGenerator(_activity, tp1, tp2,
                                               ShiftCategoryFactory.CreateShiftCategory("Test"));
        }

        private IList<WorkShiftCollection> CreateWorkShiftList()
        {
            IActivity breakActivity = ActivityFactory.CreateActivity("lunch");
            DateTimePeriod breakPeriod = new DateTimePeriod(new DateTime(1800, 1, 1, 4, 0, 0, DateTimeKind.Utc), new DateTime(1800, 1, 1, 5, 0, 0, DateTimeKind.Utc));

            WorkShift ws1 = CreateWorkShift(TimeSpan.FromHours(1), TimeSpan.FromHours(8), _activity, _category);
            ws1.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod));
            WorkShift ws2 = CreateWorkShift(TimeSpan.FromHours(1), TimeSpan.FromHours(9), _activity, _category);
            ws2.LayerCollection.Add(new WorkShiftActivityLayer(breakActivity, breakPeriod));
	        var listOfWorkShifts = new WorkShiftCollection(null) {ws1, ws2};

	        return new List<WorkShiftCollection> {listOfWorkShifts};
        }

        private static WorkShift CreateWorkShift(TimeSpan start, TimeSpan end, IActivity activity, IShiftCategory category)
        {
            WorkShift retObj = new WorkShift(category);
            retObj.LayerCollection.Add(
                new WorkShiftActivityLayer(activity,
                                           WorkShiftFactory.DateTimePeriodForWorkShift(start, end)));
            return retObj;
        }
    }
}
