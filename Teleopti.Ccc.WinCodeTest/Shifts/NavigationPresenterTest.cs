using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class NavigationPresenterTest
    {
        private NavigationPresenter _target;
        private MockRepository _mock;
        private IDataHelper _dataHelper;
        private IExplorerViewModel _model;
        private IExplorerPresenter _explorerPresenter;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IWorkShiftRuleSet _newRuleSet;
        private IRuleSetBag _newRuleSetBag;
        private readonly TypedBindingCollection<IActivity> _activities = new TypedBindingCollection<IActivity>();
        private readonly TypedBindingCollection<IShiftCategory> _categories = new TypedBindingCollection<IShiftCategory>();

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _dataHelper = _mock.StrictMock<IDataHelper>();
            _explorerPresenter = _mock.StrictMock<IExplorerPresenter>();
            _model = new ExplorerViewModel { DefaultSegment = 15 };

            _activities.Add(ActivityFactory.CreateActivity("Test"));
            _categories.Add(ShiftCategoryFactory.CreateShiftCategory("CategoryC"));
            _newRuleSet = WorkShiftRuleSetFactory.Create();
            _newRuleSetBag = new RuleSetBag();

            _ruleSetCollection = new List<IWorkShiftRuleSet>();
            _ruleSetBagCollection = new List<IRuleSetBag>();

            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());
            _ruleSetBagCollection.Add(_newRuleSetBag);

            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 16));
            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 17));

            _model.SetActivityCollection(_activities);
            _model.SetCategoryCollection(_categories);

            _model.SetRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            _model.SetRuleSetBagCollection(new ReadOnlyCollection<IRuleSetBag>(_ruleSetBagCollection));
            _model.SetFilteredRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));

            _target = new NavigationPresenter(_explorerPresenter,_dataHelper);
        }
        
        [Test]
        public void VerifyCreateDefaultRuleSet()
        {
            using (_mock.Record())
            {
                Expect.Call(_dataHelper.CreateDefaultRuleSet(_activities[0], _categories[0], _model.DefaultStartPeriod,
                                                             _model.StartPeriodSegment, _model.DefaultEndPeriod,
                                                             _model.EndPeriodSegment)).Return(_newRuleSet);
                Expect.Call(_explorerPresenter.Model).Return(_model);
            }
            using (_mock.Playback())
            {
                IWorkShiftRuleSet ruleSet = _target.CreateDefaultRuleSet();
                Assert.IsNotNull(ruleSet);
            }
        }

        [Test]
        public void VerifyCreateDefaultRuleSetBag()
        {
            using (_mock.Record())
            {
                Expect.Call(_dataHelper.CreateDefaultRuleSetBag()).Return(_newRuleSetBag);
            }
            using (_mock.Playback())
            {
                IRuleSetBag bag = _target.CreateDefaultRuleSetBag();
                Assert.IsNotNull(bag);
            }
        }

        [Test]
        public void VerifyCanRemoveRuleSet()
        {
            using (_mock.Record())
            {
                Expect.Call(_explorerPresenter.Model).Return(_model);
                Expect.Call(()=>_dataHelper.Delete(_ruleSetCollection[0]));
            }
            using (_mock.Playback())
            {
                _newRuleSetBag.AddRuleSet(_ruleSetCollection[0]);
                Assert.AreEqual(1, _ruleSetCollection[0].RuleSetBagCollection.Count);
                _target.RemoveRuleSet(_ruleSetCollection[0], null);
                //Assert.AreEqual(0, _ruleSetCollection[0].RuleSetBagCollection.Count);
                Assert.AreEqual(0, _model.RuleSetCollection.Count);
            }
        }

        [Test]
        public void VerifyCanRemoveRuleSetBag()
        {
            using (_mock.Record())
            {
                Expect.Call(_explorerPresenter.Model).Return(_model);
            }
            using (_mock.Playback())
            {
                _ruleSetBagCollection[0].AddRuleSet(_newRuleSet);
                Assert.AreEqual(1, _ruleSetBagCollection[0].RuleSetCollection.Count);
                _target.RemoveRuleSetBag(_ruleSetBagCollection[0]);
                Assert.AreEqual(0, _ruleSetBagCollection[0].RuleSetCollection.Count);
                Assert.AreEqual(0, _model.RuleSetBagCollection.Count);
            }
        }

        [Test]
        public void VerifyCanRemoveRuleSetFromParentBag()
        {
            _newRuleSetBag.AddRuleSet(_newRuleSet);
            Assert.IsTrue(_newRuleSetBag.RuleSetCollection.Contains(_newRuleSet));
            _target.RemoveRuleSet(_newRuleSet, _newRuleSetBag);
            Assert.IsFalse(_newRuleSetBag.RuleSetCollection.Contains(_newRuleSet));
        }

        [Test]
        public void VerifyCanRemoveRuleSetBagFromParentRuleSet()
        {
            _newRuleSetBag.AddRuleSet(_newRuleSet);
            Assert.IsTrue(_newRuleSetBag.RuleSetCollection.Contains(_newRuleSet));
            _target.RemoveRuleSetBag(_newRuleSetBag);
            Assert.IsFalse(_newRuleSetBag.RuleSetCollection.Contains(_newRuleSet));
        }
    }
}
