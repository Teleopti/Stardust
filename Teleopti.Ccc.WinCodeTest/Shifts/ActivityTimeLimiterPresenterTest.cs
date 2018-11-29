using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
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
    public class ActivityTimeLimiterPresenterTest
    {
        private MockRepository _mock;
        private ActivityTimeLimiterPresenter _target;
        private IDataHelper _helper;
        private IExplorerPresenter _explorer;
        private IExplorerViewModel _model;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private IActivity _activity;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _explorer = _mock.StrictMock<IExplorerPresenter>();
            _helper = _mock.StrictMock<IDataHelper>();


            _ruleSetCollection = new List<IWorkShiftRuleSet>();
            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());

            _model = new ExplorerViewModel { DefaultSegment = 15 };
            _model.SetRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            _model.SetFilteredRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));

            _activity = ActivityFactory.CreateActivity("Test");
            var activityLengthWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
            var activityPositionWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
            var auto = new AutoPositionedActivityExtender(_activity, 
                                                                                     activityLengthWithSegment,
                                                                                     TimeSpan.FromMinutes(15));

            ActivityNormalExtender absolute = new ActivityAbsoluteStartExtender(_activity,
                                                                                activityLengthWithSegment,
                                                                                activityPositionWithSegment);

            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 16));
            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 17));

            _ruleSetCollection[0].AddExtender(auto);
            _ruleSetCollection[0].AddExtender(absolute);

            ActivityTimeLimiter limiter = new ActivityTimeLimiter(_activity,
                TimeSpan.FromHours(1), 
                OperatorLimiter.Equals);

            _ruleSetCollection[0].AddLimiter(limiter);
            _target = new ActivityTimeLimiterPresenter(_explorer,_helper);
        }

        [Test]
        public void VerifyLoadModelCollection()
        {
            using (_mock.Record())
            {
                Expect.Call(_explorer.Model).Return(_model);
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                Assert.AreEqual(1, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyAddAndSaveLimiter()
        {
            ActivityTimeLimiter newLimiter = new ActivityTimeLimiter(_activity,
                TimeSpan.FromHours(1),
                OperatorLimiter.GreaterThen);

            using (_mock.Record())
            {
                Expect.Call(_explorer.Model).Return(_model).Repeat.Times(3);
                Expect.Call(_helper.CreateDefaultActivityTimeLimiter(_ruleSetCollection[0], _model.DefaultStartPeriod.SpanningTime()))
                    .Return(newLimiter);
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.AddAndSaveLimiter();
                Assert.AreEqual(2, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyDeleteLimiter()
        {
            using (_mock.Record())
            {
                Expect.Call(_explorer.Model).Return(_model).Repeat.Times(2);
                Expect.Call(() => _helper.Save(_model.RuleSetCollection[0]));
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.DeleteLimiter(new ReadOnlyCollection<int>(new List<int> {0}));
                Assert.AreEqual(0, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifySetAccessibilityDates()
        {
            ActivityTimeLimiter myLimiter = new ActivityTimeLimiter(ActivityFactory.CreateActivity("doWork"),
                TimeSpan.FromHours(1),
                OperatorLimiter.LessThen);

            _target.SetActivityTimeLimiterAdapters(new ReadOnlyCollection<IActivityTimeLimiterViewModel>(
                new List<IActivityTimeLimiterViewModel>
                    {
                    new ActivityTimeLimiterViewModel(_ruleSetCollection[0], myLimiter)
                }
            ));
            Assert.AreEqual(1, _target.ModelCollection.Count);
        }

		[Test]
	    public void ShouldClearModelWhenLoadingModelCollection()
		{
			_model = _mock.StrictMock<IExplorerViewModel>();
			using (_mock.Record())
			{
				Expect.Call(_explorer.Model).Return(_model);
				Expect.Call(_model.FilteredRuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>()));
			}
			using (_mock.Playback())
			{
				var limiter = new ActivityTimeLimiter(_activity, TimeSpan.FromHours(1), OperatorLimiter.Equals);
				var modelList = _ruleSetCollection.Select(ruleSet => new ActivityTimeLimiterViewModel(ruleSet, limiter)).Cast<IActivityTimeLimiterViewModel>().ToList();
				_target.SetModelCollection(new ReadOnlyCollection<IActivityTimeLimiterViewModel>(modelList));

				Assert.AreEqual(1, _target.ModelCollection.Count);
				_target.LoadModelCollection();
				Assert.AreEqual(0, _target.ModelCollection.Count);
			}    
	    }
    }
}
