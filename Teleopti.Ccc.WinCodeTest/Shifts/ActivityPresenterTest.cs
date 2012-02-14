using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Models;
using Teleopti.Ccc.WinCode.Shifts.Presenters;
using Teleopti.Ccc.WinCode.Shifts.Views;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class ActivityPresenterTest
    {
        private MockRepository _mock;
        private ActivityPresenter _target;
        private IDataHelper _helper;
        private IExplorerPresenter _explorer;
        private IExplorerViewModel _model;
        private IExplorerView _view;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private IActivity _activity;
        private readonly TimePeriodWithSegment _activityLengthWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
        private readonly TimePeriodWithSegment _activityPositionWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _explorer = _mock.StrictMock<IExplorerPresenter>();
            _model = _mock.StrictMock<IExplorerViewModel>();
            _helper = _mock.StrictMock<IDataHelper>();
            _view = _mock.StrictMock<IExplorerView>();

            _ruleSetCollection = new List<IWorkShiftRuleSet>();
            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());

            _activity = ActivityFactory.CreateActivity("Test");
            
            AutoPositionedActivityExtender auto = new AutoPositionedActivityExtender(_activity, 
                                                                                     _activityLengthWithSegment,
                                                                                     TimeSpan.FromMinutes(15));

            ActivityNormalExtender absolute = new ActivityAbsoluteStartExtender(_activity,
                                                                                _activityLengthWithSegment,
                                                                                _activityPositionWithSegment);

            _ruleSetCollection[0].AddAccessibilityDate(new DateTime(2009, 02, 16).ToUniversalTime());
            _ruleSetCollection[0].AddAccessibilityDate(new DateTime(2009, 02, 17).ToUniversalTime());

            _ruleSetCollection[0].AddExtender(auto);
            _ruleSetCollection[0].AddExtender(absolute);
            
            /*using (_mock.Record())
            {
                Expect
                    .On(_model)
                    .Call(_model.ActivityCollection)
                    .Return(activities)
                    .Repeat.Any();

                Expect
                    .On(_model)
                    .Call(_model.DefaultSegment)
                    .Return(15)
                    .Repeat.Any();

                Expect
                    .On(_model)
                    .Call(_model.RuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection))
                    .Repeat.Any();

                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection))
                    .Repeat.Any();

                Expect
                    .On(_explorer)
                    .Call(_explorer.DataWorkHelper)
                    .Return(_helper)
                    .Repeat.Any();

                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model)
                    .Repeat.Any();

                
                    .Repeat.Any();

                
                LastCall.IgnoreArguments().Repeat.Any();
            }*/
            _target = new ActivityPresenter(_explorer,_helper);
        }

        [Test]
        public void VerifyLoadModelCollection()
        {
            using (_mock.Record())
            {
                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model);
                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                Assert.AreEqual(2, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyAddAbsolutePositionActivity()
        {
            TypedBindingCollection<IActivity> activities = new TypedBindingCollection<IActivity>();
            activities.Add(_activity);

            using (_mock.Record())
            {
                Expect
                    .Call(_explorer.Model)
                    .Return(_model).Repeat.Times(6);
                Expect
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection)).Repeat.Times(3);
                Expect
                    .Call(_model.ActivityCollection)
                    .Return(activities);
                Expect.Call(_model.DefaultSegment)
                    .Return(15).Repeat.Twice();
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.AddAbsolutePositionActivity();
                Assert.AreEqual(3, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyDeleteActivities()
        {
            using (_mock.Record())
            {
                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model);
                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.DeleteActivities(new ReadOnlyCollection<int>(new List<int> {1, 2}));
                Assert.AreEqual(0, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyReorderActivities()
        {
            using (_mock.Record())
            {
                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model);
                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.ReOrderActivities(new ReadOnlyCollection<int>(new List<int> {2}), MoveType.MoveUp);
                Assert.AreEqual(typeof (AbsolutePositionViewModel), _target.ModelCollection[0].GetType());
                Assert.AreEqual(typeof (AutoPositionViewModel), _target.ModelCollection[1].GetType());
            }
        }

        [Test]
        public void VerifyActivityTypeChangedEvent()
        {
            using (_mock.Record())
            {
                Expect.Call(_explorer.Model)
                    .Return(_model).Repeat.Times(2);
                Expect.Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
                Expect.Call(_model.DefaultSegment)
                    .Return(15);
                Expect.Call(_explorer.View)
                    .Return(_view).Repeat.Twice();
                Expect.Call(_view.RefreshActivityGridView).Repeat.Twice();
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.ModelCollection[0].IsAutoPosition = false;
                Assert.AreEqual(typeof (AbsolutePositionViewModel), _target.ModelCollection[0].GetType());

                _target.ModelCollection[0].IsAutoPosition = true;
                Assert.AreEqual(typeof (AutoPositionViewModel), _target.ModelCollection[0].GetType());
            }
        }

        [Test]
        public void VerifyChangeExtenderType()
        {
            using (_mock.Record())
            {
                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model).Repeat.Times(2);
                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection)).Repeat.Times(2);
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();

                Assert.AreEqual(typeof (ActivityAbsoluteStartExtender),
                                _target.ModelCollection[1].WorkShiftExtender.GetType());
                _target.ChangeExtenderType((AbsolutePositionViewModel) _target.ModelCollection[1],
                                           new ActivityRelativeStartExtender(_activity, _activityLengthWithSegment,
                                                                             _activityPositionWithSegment)
                    );
                Assert.AreEqual(typeof (ActivityRelativeStartExtender),
                                _target.ModelCollection[1].WorkShiftExtender.GetType());
            }
        }
    }
}
