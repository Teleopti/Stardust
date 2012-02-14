using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class EditExtendedPreferencePresenterTest
    {
        private EditExtendedPreferencePresenter _target;
        private MockRepository _mocks;
        private IEditExtendedPreferenceView _view;
        private IExtendedPreferenceModel _model;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IEditExtendedPreferenceView>();
            _model = _mocks.StrictMock<IExtendedPreferenceModel>();
            _target = new EditExtendedPreferencePresenter(_view, _model);
        }

        [Test]
        public void ShouldInitialize()
        {
            _mocks.Record();

            var activities = new Activity[]{};

            _view.StartTimeLimitationErrorMessage = null;
            _view.EndTimeLimitationErrorMessage = null;
            _view.WorkTimeLimitationErrorMessage = null;
            _view.ActivityStartTimeLimitationErrorMessage = null;
            _view.ActivityEndTimeLimitationErrorMessage = null;
            _view.ActivityTimeLimitationErrorMessage = null;
            _view.DayOffEnabled = true;
            _view.ShiftCategoryEnabled = true;
            _view.AbsenceEnabled = true;
            _view.ShiftTimeControlsEnabled = false;
            _view.ActivityEnabled = true;
            _view.ActivityTimeControlsEnabled = false;
            _view.SaveButtonEnabled = false;
            Expect.Call(_model.AllowedPreferenceActivities).Return(activities);
            Expect.Call(() => _view.PopulateActivities(activities));
            Expect.Call(_model.ModifyExtendedPreferencesIsPermitted).Return(true);

            _mocks.ReplayAll();

            _target.Initialize();

            _mocks.VerifyAll();

        }

        [Test]
        public void ShouldSetModelValuesToView()
        {
            _mocks.Record();

            var dayOff = new DayOff("dayOff", "do", "id", Color.Black);
            var shiftCategory = new ShiftCategory("shiftCat", "ca", "id", Color.Black);
            var activity = new Activity("id", "act");
            var absence = new Absence("name", "shortName", "id", Color.Empty);

            Expect.Call(_model.StartTimeLimitationMin).Return(TimeSpan.FromHours(1));
            Expect.Call(_model.StartTimeLimitationMax).Return(TimeSpan.FromHours(2));
            Expect.Call(_model.EndTimeLimitationMin).Return(TimeSpan.FromHours(3)).Repeat.Twice();
            Expect.Call(_model.EndTimeLimitationMax).Return(TimeSpan.FromHours(4)).Repeat.Twice();
            Expect.Call(_model.WorkTimeLimitationMin).Return(TimeSpan.FromHours(5));
            Expect.Call(_model.WorkTimeLimitationMax).Return(TimeSpan.FromHours(6));
            Expect.Call(_model.EndTimeLimitationMaxNextDay).Return(false);
            Expect.Call(_model.EndTimeLimitationMinNextDay).Return(false);
            Expect.Call(_model.DayOff).Return(dayOff);
            Expect.Call(_model.ShiftCategory).Return(shiftCategory);
            Expect.Call(_model.Absence).Return(absence);
            Expect.Call(_model.Activity).Return(activity);
            Expect.Call(_model.ActivityStartTimeLimitationMin).Return(TimeSpan.FromHours(7));
            Expect.Call(_model.ActivityStartTimeLimitationMax).Return(TimeSpan.FromHours(8));
            Expect.Call(_model.ActivityEndTimeLimitationMin).Return(TimeSpan.FromHours(9));
            Expect.Call(_model.ActivityEndTimeLimitationMax).Return(TimeSpan.FromHours(10));
            Expect.Call(_model.ActivityTimeLimitationMin).Return(TimeSpan.FromHours(11));
            Expect.Call(_model.ActivityTimeLimitationMax).Return(TimeSpan.FromHours(12));

            _view.StartTimeLimitationMin = TimeSpan.FromHours(1);
            _view.StartTimeLimitationMax = TimeSpan.FromHours(2);
            _view.EndTimeLimitationMin = TimeSpan.FromHours(3);
            _view.EndTimeLimitationMax = TimeSpan.FromHours(4);
            _view.WorkTimeLimitationMin = TimeSpan.FromHours(5);
            _view.WorkTimeLimitationMax = TimeSpan.FromHours(6);
            _view.EndTimeLimitationMaxNextDay = false;
            _view.EndTimeLimitationMinNextDay = false;
            _view.EndTimeLimitationMinNextDayEnabled = true;
            _view.EndTimeLimitationMaxNextDayEnabled = true;
            _view.DayOff = dayOff;
            _view.ShiftCategory = shiftCategory;
            _view.Absence = absence;
            _view.Activity = activity;
            _view.ActivityStartTimeLimitationMin = TimeSpan.FromHours(7);
            _view.ActivityStartTimeLimitationMax = TimeSpan.FromHours(8);
            _view.ActivityEndTimeLimitationMin = TimeSpan.FromHours(9);
            _view.ActivityEndTimeLimitationMax = TimeSpan.FromHours(10);
            _view.ActivityTimeLimitationMin = TimeSpan.FromHours(11);
            _view.ActivityTimeLimitationMax = TimeSpan.FromHours(12);

            _mocks.ReplayAll();

            _target.SetModelValuesToView();

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetViewValuesToModel()
        {
            _mocks.Record();

            var dayOff = new DayOff("dayOff", "do", "id", Color.Black);
            var shiftCategory = new ShiftCategory("shiftCat", "ca", "id", Color.Black);
            var activity = new Activity("id", "act");
            var absence = new Absence("name", "shortName", "id", Color.Empty);

            Expect.Call(_view.DayOff).Return(dayOff);
            Expect.Call(_view.ShiftCategory).Return(shiftCategory);
            Expect.Call(_view.Absence).Return(absence);
            Expect.Call(_view.StartTimeLimitationMin).Return(TimeSpan.FromHours(1));
            Expect.Call(_view.StartTimeLimitationMax).Return(TimeSpan.FromHours(2));
            Expect.Call(_view.EndTimeLimitationMin).Return(TimeSpan.FromHours(3));
            Expect.Call(_view.EndTimeLimitationMax).Return(TimeSpan.FromHours(4));
            Expect.Call(_view.WorkTimeLimitationMin).Return(TimeSpan.FromHours(5));
            Expect.Call(_view.WorkTimeLimitationMax).Return(TimeSpan.FromHours(6));
            Expect.Call(_view.EndTimeLimitationMaxNextDay).Return(false);
            Expect.Call(_view.EndTimeLimitationMinNextDay).Return(false);
            Expect.Call(_view.Activity).Return(activity);
            Expect.Call(_view.ActivityStartTimeLimitationMin).Return(TimeSpan.FromHours(7));
            Expect.Call(_view.ActivityStartTimeLimitationMax).Return(TimeSpan.FromHours(8));
            Expect.Call(_view.ActivityEndTimeLimitationMin).Return(TimeSpan.FromHours(9));
            Expect.Call(_view.ActivityEndTimeLimitationMax).Return(TimeSpan.FromHours(10));
            Expect.Call(_view.ActivityTimeLimitationMin).Return(TimeSpan.FromHours(11));
            Expect.Call(_view.ActivityTimeLimitationMax).Return(TimeSpan.FromHours(12));

            _model.DayOff = dayOff;
            _model.ShiftCategory = shiftCategory;
            _model.Absence = absence;
            _model.StartTimeLimitationMin = TimeSpan.FromHours(1);
            _model.StartTimeLimitationMax = TimeSpan.FromHours(2);
            _model.EndTimeLimitationMin = TimeSpan.FromHours(3);
            _model.EndTimeLimitationMax = TimeSpan.FromHours(4);
            _model.WorkTimeLimitationMin = TimeSpan.FromHours(5);
            _model.WorkTimeLimitationMax = TimeSpan.FromHours(6);
            _model.EndTimeLimitationMaxNextDay = false;
            _model.EndTimeLimitationMinNextDay = false;
            _model.Activity = activity;
            _model.ActivityStartTimeLimitationMin = TimeSpan.FromHours(7);
            _model.ActivityStartTimeLimitationMax = TimeSpan.FromHours(8);
            _model.ActivityEndTimeLimitationMin = TimeSpan.FromHours(9);
            _model.ActivityEndTimeLimitationMax = TimeSpan.FromHours(10);
            _model.ActivityTimeLimitationMin = TimeSpan.FromHours(11);
            _model.ActivityTimeLimitationMax = TimeSpan.FromHours(12);

            _mocks.ReplayAll();

            _target.SetViewValuesToModel();

            _mocks.VerifyAll();
        }
    }
}