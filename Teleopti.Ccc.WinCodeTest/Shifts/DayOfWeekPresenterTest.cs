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
    public class DayOfWeekPresenterTest
    {
        private MockRepository _mock;
        private DaysOfWeekPresenter _target;
        private IDataHelper _helper;
        private IExplorerPresenter _explorer;
        private IExplorerViewModel _model;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _explorer = _mock.StrictMock<IExplorerPresenter>();
            _model = _mock.StrictMock<IExplorerViewModel>();
            _helper = _mock.StrictMock<IDataHelper>();

            _ruleSetCollection = new List<IWorkShiftRuleSet>();
            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());

            IActivity activity = ActivityFactory.CreateActivity("Test");
            TimePeriodWithSegment activityLengthWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
            TimePeriodWithSegment activityPositionWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, 15);
            AutoPositionedActivityExtender auto = new AutoPositionedActivityExtender(activity, 
                                                                                     activityLengthWithSegment,
                                                                                     TimeSpan.FromMinutes(15));

            ActivityNormalExtender absolute = new ActivityAbsoluteStartExtender(activity,
                                                                                activityLengthWithSegment,
                                                                                activityPositionWithSegment);

            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 16));
            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 17));

            _ruleSetCollection[0].AddExtender(auto);
            _ruleSetCollection[0].AddExtender(absolute);

            ActivityTimeLimiter limiter = new ActivityTimeLimiter(activity,
                TimeSpan.FromHours(1), 
                OperatorLimiter.Equals);

            _ruleSetCollection[0].AddLimiter(limiter);

            _ruleSetCollection[0].AddAccessibilityDayOfWeek(DayOfWeek.Monday);
            _ruleSetCollection[0].AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
            
            /*using (_mock.Record())
            {
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
            }*/
            _target = new DaysOfWeekPresenter(_explorer,_helper);
        }

        [Test]
        public void VerifyLoadModelCollection()
        {
            using (_mock.Record())
            {
                Expect.Call(_explorer.Model).Return(_model);
                Expect.Call(_model.FilteredRuleSetCollection).Return(
                    new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                Assert.AreEqual(1, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifySetDaysOfWeekCollection()
        {
            _target.SetDaysOfWeekCollection(
                new List<IDaysOfWeekViewModel>
                    {
                    new DaysOfWeekViewModel(_ruleSetCollection[0])
                });

            Assert.AreEqual(1, _target.ModelCollection.Count);
        }

        [Test]
        public void VerifyValidate()
        {
            Assert.IsTrue(_target.Validate());
        }

		[Test]
	    public void ShouldClearModelWhenLoadingModelCollection()
	    {
			using (_mock.Record())
			{
				Expect.Call(_explorer.Model).Return(_model);
				Expect.Call(_model.FilteredRuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>()));
			}
			using (_mock.Playback())
			{
				var modelList = _ruleSetCollection.Select(ruleSet => new DaysOfWeekViewModel(ruleSet)).Cast<IDaysOfWeekViewModel>().ToList();
				_target.SetModelCollection(new ReadOnlyCollection<IDaysOfWeekViewModel>(modelList));

				Assert.AreEqual(1, _target.ModelCollection.Count);
				_target.LoadModelCollection();
				Assert.AreEqual(0, _target.ModelCollection.Count);
			}   
	    }
    }
}
