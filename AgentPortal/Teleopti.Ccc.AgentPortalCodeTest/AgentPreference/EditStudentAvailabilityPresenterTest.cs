using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class EditStudentAvailabilityPresenterTest
    {
        private EditStudentAvailabilityPresenter _target;
        private MockRepository _mocks;
        private IEditStudentAvailabilityView _view;
        private IEditStudentAvailabilityModel _model;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IEditStudentAvailabilityView>();
            _model = _mocks.StrictMock<IEditStudentAvailabilityModel>();
            _target = new EditStudentAvailabilityPresenter(_view, _model);
        }

        [Test]
        public void ShouldInitialize()
        {
            _mocks.Record();

            _view.StartTimeLimitationErrorMessage = null;
            _view.EndTimeLimitationErrorMessage = null;
            _view.SecondStartTimeLimitationErrorMessage = null;
            _view.SecondEndTimeLimitationErrorMessage = null;
            _view.SaveButtonEnabled = false;

            Expect.Call(_model.CreateStudentAvailabilityIsPermitted).Return(true);

            _mocks.ReplayAll();

            _target.Initialize();

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldInitializeWithNoPermissionToCreateStudentAvailability()
        {
            _mocks.Record();

            _view.StartTimeLimitationErrorMessage = null;
            _view.EndTimeLimitationErrorMessage = null;
            _view.SecondStartTimeLimitationErrorMessage = null;
            _view.SecondEndTimeLimitationErrorMessage = null;
            _view.SaveButtonEnabled = false;
            _view.HideView();

            Expect.Call(_model.CreateStudentAvailabilityIsPermitted).Return(false);

            _mocks.ReplayAll();

            _target.Initialize();

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetModelValuesToView()
        {
            _mocks.Record();

            Expect.Call(_model.StartTimeLimitation).Return(TimeSpan.FromHours(1));
            Expect.Call(_model.SecondStartTimeLimitation).Return(TimeSpan.FromHours(2));
            Expect.Call(_model.EndTimeLimitation).Return(TimeSpan.FromHours(3));
            Expect.Call(_model.SecondEndTimeLimitation).Return(TimeSpan.FromHours(4));
            Expect.Call(_model.EndTimeLimitationNextDay).Return(false);
            Expect.Call(_model.SecondEndTimeLimitationNextDay).Return(false);

            _view.StartTimeLimitation = TimeSpan.FromHours(1);
            _view.SecondStartTimeLimitation = TimeSpan.FromHours(2);
            _view.EndTimeLimitation = TimeSpan.FromHours(3);
            _view.SecondEndTimeLimitation = TimeSpan.FromHours(4);
            _view.EndTimeLimitationNextDay = false;
            _view.SecondEndTimeLimitationNextDay = false;

            _mocks.ReplayAll();

            _target.SetModelValuesToView();

            _mocks.VerifyAll();
        }
        
        [Test]
        public void ShouldSetViewValuesToModel()
        {
            _mocks.Record();

            Expect.Call(_view.StartTimeLimitation).Return(TimeSpan.FromHours(1));
            Expect.Call(_view.SecondStartTimeLimitation).Return(TimeSpan.FromHours(2));
            Expect.Call(_view.EndTimeLimitation).Return(TimeSpan.FromHours(3));
            Expect.Call(_view.SecondEndTimeLimitation).Return(TimeSpan.FromHours(4));
            Expect.Call(_view.EndTimeLimitationNextDay).Return(false);
            Expect.Call(_view.SecondEndTimeLimitationNextDay).Return(false);

            _model.StartTimeLimitation = TimeSpan.FromHours(1);
            _model.SecondStartTimeLimitation = TimeSpan.FromHours(2);
            _model.EndTimeLimitation = TimeSpan.FromHours(3);
            _model.SecondEndTimeLimitation = TimeSpan.FromHours(4);
            _model.EndTimeLimitationNextDay = false;
            _model.SecondEndTimeLimitationNextDay = false;

            _mocks.ReplayAll();

            _target.SetViewValuesToModel();

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotBePossibleToSaveTimesToNoneNone()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _view.StartTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.EndTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.SecondStartTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.SecondEndTimeLimitationErrorMessage = null);

                Expect.Call(_view.StartTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_view.EndTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(() => _view.StartTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);
                Expect.Call(() => _view.EndTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);

                Expect.Call(_view.SecondStartTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_view.SecondEndTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(() => _view.SecondStartTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);
                Expect.Call(() => _view.SecondEndTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);
            }

            using(_mocks.Playback())
            {
                _target.Save();
            }
        }

        [Test]
        public void ShouldNotBePossibleToSaveStartTimeToNone()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _view.StartTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.EndTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.SecondStartTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.SecondEndTimeLimitationErrorMessage = null);

                Expect.Call(_view.StartTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_view.EndTimeLimitation).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
                Expect.Call(() => _view.StartTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);

                Expect.Call(_view.SecondStartTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_view.SecondEndTimeLimitation).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
                Expect.Call(() => _view.SecondStartTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);
            }

            using (_mocks.Playback())
            {
                _target.Save();
            }   
        }

        [Test]
        public void ShouldNotBePossibleToSaveEndTimeToNone()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _view.StartTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.EndTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.SecondStartTimeLimitationErrorMessage = null);
                Expect.Call(() => _view.SecondEndTimeLimitationErrorMessage = null);

                Expect.Call(_view.StartTimeLimitation).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
                Expect.Call(_view.EndTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(() => _view.EndTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);

                Expect.Call(_view.SecondStartTimeLimitation).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
                Expect.Call(_view.SecondEndTimeLimitation).Return(null).Repeat.AtLeastOnce();
                Expect.Call(() => _view.SecondEndTimeLimitationErrorMessage = Resources.MustSpecifyValidTime);
            }

            using (_mocks.Playback())
            {
                _target.Save();
            }      
        }
    }
}