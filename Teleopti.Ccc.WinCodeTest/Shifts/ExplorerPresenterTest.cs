using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using MbCache.Core;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class ExplorerPresenterTest
    {
        private MockRepository _mock;
        private IUnitOfWork _uow;
        private IDataHelper _helper;
        private ExplorerPresenter _target;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        //private IExplorerViewModel _model;
        private IExplorerView _view;
        private IActivity _activity;
        private readonly TimePeriodWithSegment _activityLengthWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
        private readonly TimePeriodWithSegment _activityPositionWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
        private readonly TypedBindingCollection<IActivity> _activities = new TypedBindingCollection<IActivity>();
        private readonly TypedBindingCollection<IShiftCategory> _categories = new TypedBindingCollection<IShiftCategory>();
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IEventAggregator _eventAggregator;
        private IMbCacheFactory _mbCacheFactory;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            //_model = _mock.StrictMock<IExplorerViewModel>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _eventAggregator = new EventAggregator();
            _mbCacheFactory = _mock.StrictMock<IMbCacheFactory>();
            _uow = _mock.StrictMock<IUnitOfWork>();
            _helper = _mock.StrictMock<IDataHelper>();
            _view = _mock.StrictMock<IExplorerView>();
            _ruleSetCollection = new List<IWorkShiftRuleSet> {WorkShiftRuleSetFactory.Create()};
            _ruleSetBagCollection = new List<IRuleSetBag> {new RuleSetBag()};
            _activity = ActivityFactory.CreateActivity("Test");
            _activities.Add(_activity);

            var auto = new AutoPositionedActivityExtender(_activity,_activityLengthWithSegment,
                                                                     TimeSpan.FromMinutes(15));

            ActivityNormalExtender absolute = new ActivityAbsoluteStartExtender(_activity,
                                                                                _activityLengthWithSegment,
                                                                                _activityPositionWithSegment);

            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 16));
            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 17));

            _ruleSetCollection[0].AddExtender(auto);
            _ruleSetCollection[0].AddExtender(absolute);

            _categories.Add(ShiftCategoryFactory.CreateShiftCategory("CategoryA"));
            _categories.Add(ShiftCategoryFactory.CreateShiftCategory("CategoryB"));
            _categories.Add(ShiftCategoryFactory.CreateShiftCategory("CategoryC"));

            var model = new ExplorerViewModel
            {
                DefaultSegment = 15,
                RuleSetCollection =
                    new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>()),
                RuleSetBagCollection = new ReadOnlyCollection<IRuleSetBag>(new List<IRuleSetBag>())
            };

				_target = new ExplorerPresenter(_view, _helper, new RuleSetProjectionEntityService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())),
                _unitOfWorkFactory, _eventAggregator, _mbCacheFactory, model);
            
            
        }

        private void setLoadExpectations()
        {
            

            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
            
            Expect
                .On(_helper)
                .Call(_helper.FindAllActivities(_uow))
                .Return(_activities)
                .Repeat.Any();

            Expect
                .On(_helper)
                .Call(_helper.FindAllCategories(_uow))
                .Return(_categories)
                .Repeat.Any();

            Expect
                .On(_helper)
                .Call(_helper.FindAllOperatorLimits())
                .Return(findAllOperatorLimits())
                .Repeat.Any();

            Expect
                .On(_helper)
                .Call(_helper.FindRuleSets(_uow))
                .Return(_ruleSetCollection)
                .Repeat.Any();

            Expect
                .On(_helper)
                .Call(_helper.FindRuleSetBags(_uow))
                .Return(_ruleSetBagCollection)
                .Repeat.Any();

            Expect
                .On(_helper)
                .Call(_helper.FindAllAccessibilities())
                .Return(findAllAccessibilities())
                .Repeat.Any();

            Expect.Call(_uow.Dispose);

            _view.RefreshActivityGridView();
            LastCall.IgnoreArguments().Repeat.Any();
            
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanReadProperties()
        {
            Assert.AreEqual(_view, _target.View);

            Assert.IsNotNull(_target.NavigationPresenter);
            Assert.IsNotNull(_target.VisualizePresenter);
            Assert.IsNotNull(_target.GeneralPresenter);
        }

        [Test] public void VerifyLoadMethods()
        {
            Expect.Call(_helper.DefaultSegment()).Return(15);

            setLoadExpectations();

            Expect.Call(() => _view.Show(_target, null));
            _mock.ReplayAll();

            _target.Show(null);

            Assert.AreEqual(_activities.Count, _target.Model.ActivityCollection.Count);
            Assert.AreEqual(_categories.Count, _target.Model.CategoryCollection.Count);
            Assert.AreEqual(5, _target.Model.OperatorLimitCollection.Count);
            Assert.AreEqual(1, _target.Model.RuleSetCollection.Count);
            Assert.AreEqual(1, _target.Model.RuleSetBagCollection.Count);
            Assert.AreEqual(2, _target.Model.AccessibilityCollection.Count);
            Assert.AreEqual(3, _target.Model.ClassTypeCollection.Count);
            _mock.VerifyAll();
        }

        [Test]
        public void VerifyValidate()
        {
            Assert.IsTrue(_target.Validate());
        }

        private static ReadOnlyCollection<string> findAllOperatorLimits()
        {
            IList<KeyValuePair<OperatorLimiter, string>> operatorLimitCollection = LanguageResourceHelper.TranslateEnumToList<OperatorLimiter>();
            List<string> defaultAccessibilityNames = (from p in operatorLimitCollection select p.Value).ToList();
            return new ReadOnlyCollection<string>(defaultAccessibilityNames);
        }

        private static ReadOnlyCollection<string> findAllAccessibilities()
        {
            IList<KeyValuePair<DefaultAccessibility, string>> defaultAccessibilityList = LanguageResourceHelper.TranslateEnumToList<DefaultAccessibility>();
            string[] defaultAccessibilityNames = (from p in defaultAccessibilityList select p.Value).ToArray();
            return new ReadOnlyCollection<string>(defaultAccessibilityNames);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Common.IViewBase.ShowErrorMessage(System.String,System.String)"), Test]
        public void ShouldHandleOptimisticLockExceptionOnPersist()
        {
            Expect.Call(() => _helper.PersistAll()).Throw(
                new OptimisticLockException());
            Expect.Call(
                () =>
                _view.ShowErrorMessage(
                    string.Concat(
                        UserTexts.Resources.SomeoneElseHaveChanged + "." + UserTexts.Resources.PleaseTryAgainLater, " "),
                    UserTexts.Resources.Message));
            
            setLoadExpectations();

            Expect.Call(() => _view.RefreshChildViews());
            Expect.Call(() => _view.ForceRefreshNavigationView());

            _mock.ReplayAll();
            _target.Persist().Should().Be.False();
            _mock.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ajajaj"), Test]
        public void ShouldHandleDataSourceExceptionOnPersist()
        {
            var exception = new DataSourceException("ajajaj");
            Expect.Call(() => _helper.PersistAll()).Throw(
                exception);
            _view.ShowDataSourceException(exception);

            _mock.ReplayAll();
            _target.Persist().Should().Be.False();
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueOnPersistWhenNoError()
        {
            Expect.Call(() => _helper.PersistAll());
            Expect.Call(_view.UpdateNavigationViewTreeIcons);
            _mock.ReplayAll();
            _target.Persist().Should().Be.True();
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueOnCheckForUnsavedIfNoUnsaved()
        {
            Expect.Call(_helper.HasUnsavedData()).Return(false);
            _mock.ReplayAll();
            _target.CheckForUnsavedData().Should().Be.True();
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseOnCheckForUnsavedOnCancel()
        {
            Expect.Call(_helper.HasUnsavedData()).Return(true);
            Expect.Call(_view.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade,
                                                      UserTexts.Resources.Shifts)).Return(DialogResult.Cancel);
            _mock.ReplayAll();
            _target.CheckForUnsavedData().Should().Be.False();
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueOnCheckForUnsavedOnNo()
        {
            Expect.Call(_helper.HasUnsavedData()).Return(true);
            Expect.Call(_view.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade,
                                                      UserTexts.Resources.Shifts)).Return(DialogResult.No);
            _mock.ReplayAll();
            _target.CheckForUnsavedData().Should().Be.True();
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldAskAndPersistOnCheckForUnsavedIfUnsavedAndYes()
        {
            Expect.Call(_helper.HasUnsavedData()).Return(true);
            Expect.Call(_view.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade,
                                                      UserTexts.Resources.Shifts)).Return(DialogResult.Yes);
            Expect.Call(() => _helper.PersistAll());
            Expect.Call(_view.UpdateNavigationViewTreeIcons);
            _mock.ReplayAll();
            _target.CheckForUnsavedData().Should().Be.True();
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldInvalidateCacheOnRuleSetChange()
        {
            var ruleSet = _mock.StrictMock<IWorkShiftRuleSet>();

            Expect.Call(() => _mbCacheFactory.Invalidate<IRuleSetProjectionEntityService>());
            _mock.ReplayAll();
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(new List<IWorkShiftRuleSet>{ruleSet});
            _mock.VerifyAll();
        }
    }
}
