using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentStudentAvailability
{
    [TestFixture]
    public class EditExtendedStudentAvailabilityPresenterTest
    {
        private EditExtendedStudentAvailabilityPresenter _target;
        private MockRepository _mocks;
        private IEditExtendedStudentAvailabilityView _view;
        private IExtendedStudentAvailabilityModel _model;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IEditExtendedStudentAvailabilityView>();
            _model = _mocks.StrictMock<IExtendedStudentAvailabilityModel>();
            _target = new EditExtendedStudentAvailabilityPresenter(_view, _model);
        }

        [Test]
        public void ShouldInitialize()
        {
            _mocks.Record();

            _view.EndTimeLimitationErrorMessage = null;
            _view.SaveButtonEnabled = false;

            _mocks.ReplayAll();

            _target.Initialize();

            _mocks.VerifyAll();

        }

        [Test]
        public void ShouldSetModelValuesToView()
        {
            _mocks.Record();

            Expect.Call(_model.StartTimeLimitation).Return(TimeSpan.FromHours(1));
            Expect.Call(_model.EndTimeLimitation).Return(TimeSpan.FromHours(3)).Repeat.Twice();
            Expect.Call(_model.EndTimeLimitationNextDay).Return(false);
            Expect.Call(_model.SecondStartTimeLimitation).Return(TimeSpan.FromHours(1));
            Expect.Call(_model.SecondEndTimeLimitation).Return(TimeSpan.FromHours(3)).Repeat.Twice();
            Expect.Call(_model.SecondEndTimeLimitationNextDay).Return(false);
           

            _view.StartTimeLimitation = TimeSpan.FromHours(1);
            _view.EndTimeLimitation = TimeSpan.FromHours(3);
            _view.EndTimeLimitationNextDay = false;
            //_view.EndTimeLimitationNextDayEnabled = true;
            
            _mocks.ReplayAll();

            _target.SetModelValuesToView();

            _mocks.VerifyAll();
        }
    }
}