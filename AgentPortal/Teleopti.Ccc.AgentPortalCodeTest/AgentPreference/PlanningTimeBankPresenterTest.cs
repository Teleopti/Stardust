using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class PlanningTimeBankPresenterTest

    {
        private MockRepository _mocks;
        private IPlanningTimeBankView _view;
        private IPlanningTimeBankModel _model;
        private PlanningTimeBankPresenter _target;
        private IPlanningTimeBankHelper _helper;
        readonly CultureInfo _info = CultureInfo.GetCultureInfo("sv-SE");
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IPlanningTimeBankView>();
            _model = _mocks.StrictMock<IPlanningTimeBankModel>();
            _helper = _mocks.StrictMock<IPlanningTimeBankHelper>();
            _target = new PlanningTimeBankPresenter(_view, _model, _helper, _info);
        }

        [Test]
        public void ShouldSetValuesInViewOnShow()
        {
            Expect.Call(_model.BalanceIn).Return(TimeSpan.FromHours(5));
            Expect.Call(() => _view.SetBalanceInText("5:00"));

            Expect.Call(_model.BalanceOut).Return(TimeSpan.FromHours(-3));
            Expect.Call(_view.BalanceOut = TimeSpan.FromHours(-3));
            Expect.Call(_model.TimeBankMax).Return(TimeSpan.FromHours(40));
            Expect.Call(() => _view.SetMaxBalanceOut(TimeSpan.FromHours(40)));

            Expect.Call(_model.TimeBankMin).Return(TimeSpan.FromHours(-6));
            Expect.Call(_model.TimeBankMax).Return(TimeSpan.FromHours(5));
            Expect.Call(() => _view.SetInfoMinMaxBalance("some string")).IgnoreArguments();

            Expect.Call(_model.CanSetBalanceOut).Return(true);
            Expect.Call(() => _view.SetEnableStateOnSave(true));
            _mocks.ReplayAll();
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetErrorInViewOnFailedValidationOnTooLow()
        {
            Expect.Call(() => _view.SetErrorText("")).IgnoreArguments();
            Expect.Call(_model.TimeBankMin).Return(TimeSpan.FromHours(-5));
            Expect.Call(_view.BalanceOut).Return(TimeSpan.FromHours(-6));
            Expect.Call(() => _view.SetErrorText("some text")).IgnoreArguments();
            _mocks.ReplayAll();
            _target.Save();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetErrorInViewOnFailedValidationOnTooHigh()
        {
            Expect.Call(() => _view.SetErrorText("")).IgnoreArguments();
            Expect.Call(_view.BalanceOut).Return(TimeSpan.FromHours(41));
            Expect.Call(_model.TimeBankMin).Return(TimeSpan.FromHours(-5));

            Expect.Call(_model.TimeBankMax).Return(TimeSpan.FromHours(40));
            Expect.Call(() => _view.SetErrorText("some text")).IgnoreArguments();
            _mocks.ReplayAll();
            _target.Save();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldEmptyErrorInViewOnValidation()
        {
            Expect.Call(() => _view.SetErrorText("")).IgnoreArguments();
            Expect.Call(_view.BalanceOut).Return(TimeSpan.FromHours(5)).Repeat.Twice();
            Expect.Call(_model.TimeBankMin).Return(TimeSpan.FromHours(-5));
            Expect.Call(_model.TimeBankMax).Return(TimeSpan.FromHours(40));
            Expect.Call(() =>_helper.SaveWantedBalanceOut(TimeSpan.FromHours(5)));
            _mocks.ReplayAll();
            _target.Save();
            _mocks.VerifyAll();
        }
    }

    
}