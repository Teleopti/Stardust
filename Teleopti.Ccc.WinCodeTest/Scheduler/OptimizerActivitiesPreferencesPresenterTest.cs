using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.Domain.ResourceCalculation;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using System;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class OptimizerActivitiesPreferencesPresenterTest
    {
        private MockRepository _mock;
        private IOptimizerActivitiesPreferences _model;
        private IOptimizerActivitiesPreferencesView _view;
        private OptimizerActivitiesPreferencesPresenter _presenter;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _model = new OptimizerActivitiesPreferences();
            _view = _mock.StrictMock<IOptimizerActivitiesPreferencesView>();
            _presenter = new OptimizerActivitiesPreferencesPresenter(_model, _view, 15);
        }

        [Test]
        public void VerifyInitialize()
        {
            using (_mock.Record())
            {
                _view.Initialize(15, _model.Activities, _model.DoNotMoveActivities);
                _view.KeepShiftCategory(_model.KeepShiftCategory);
                _view.KeepStartTime(_model.KeepStartTime);
                _view.KeepEndTime(_model.KeepEndTime);
                _view.KeepBetween(_model.AllowAlterBetween);
              
            }

            using (_mock.Playback())
            {
                _presenter.Initialize();
            }
        }

        [Test]
        public void VerifyOnButtonCancelClick()
        {
            using (_mock.Record())
            {
                _view.HideForm();

            }

            using (_mock.Playback())
            {
                _presenter.OnButtonCancelClick();
            }
            
            Assert.IsTrue(_presenter.IsCanceled);
        }

        [Test]
        public void VerifyOnButtonOkClick()
        {
            using (_mock.Record())
            {
                Expect.Call(_view.DoNotMoveActivities()).Return(new List<IActivity>()).Repeat.Once();
                _view.HideForm();
            }

            using (_mock.Playback())
            {
                _presenter.OnButtonOkClick(true, TimeSpan.FromHours(10), TimeSpan.FromHours(12));
            }

            Assert.IsFalse(_presenter.IsCanceled);
        }

        [Test]
        public void VerifyOnKeepShiftCategoryCheckedChanged()
        {
            _presenter.OnKeepShiftCategoryCheckedChanged(true);
            Assert.IsTrue(_model.KeepShiftCategory);

            _presenter.OnKeepShiftCategoryCheckedChanged(false);
            Assert.IsFalse(_model.KeepShiftCategory);
        }

        [Test]
        public void VerifyOnKeepStartTimeCheckedChanged()
        {
            _presenter.OnKeepStartTimeCheckedChanged(true);
            Assert.IsTrue(_model.KeepStartTime);

            _presenter.OnKeepStartTimeCheckedChanged(false);
            Assert.IsFalse(_model.KeepStartTime);
        }

        [Test]
        public void VerifyOnKeepEndTimeCheckedChanged()
        {
            _presenter.OnKeepEndTimeCheckedChanged(true);
            Assert.IsTrue(_model.KeepEndTime);

            _presenter.OnKeepEndTimeCheckedChanged(false);
            Assert.IsFalse(_model.KeepEndTime);
        }

        [Test]
        public void VerifyOnKeepBetweenChanged()
        {
            _presenter.OnKeepBetweenChanged(new TimePeriod());
            Assert.IsTrue(_model.AllowAlterBetween.HasValue);

            _presenter.OnKeepBetweenChanged(null);
            Assert.IsFalse(_model.AllowAlterBetween.HasValue);
        }

        [Test]
        public void VerifyOnDoNotMoveActivitiesChanged()
        {
             IList<IActivity> activities = new List<IActivity>();

            _presenter.OnDoNotMoveActivitiesChanged(activities);
            Assert.AreEqual(activities, _model.DoNotMoveActivities);
        }
    }
}
