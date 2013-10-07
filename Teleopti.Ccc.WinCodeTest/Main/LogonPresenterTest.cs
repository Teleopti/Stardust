using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.WinCode.Main;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
	public class LogonPresenterTest
	{
        private MockRepository _mocks;
        private ILogonView _view;
        private LogonModel _model;
        private IDataSourceHandler _dataSourceHandler;
        private ILoginInitializer _initializer;
        private ILogonLogger _logonLogger;
        private ILogOnOff _logOnOff;
        private LogonPresenter _target;
        private IServerEndpointSelector _endPointSelector;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.DynamicMock<ILogonView>();
            _model = new LogonModel();
            _dataSourceHandler = _mocks.DynamicMock<IDataSourceHandler>();
            _initializer = _mocks.DynamicMock<ILoginInitializer>();
            _logonLogger = _mocks.DynamicMock<ILogonLogger>();
            _logOnOff = _mocks.DynamicMock<ILogOnOff>();
            _endPointSelector = _mocks.DynamicMock<IServerEndpointSelector>();
            _target = new LogonPresenter(_view, _model, _dataSourceHandler, _initializer, _logonLogger, _logOnOff, _endPointSelector);
        }

        [Test]
        public void ShouldCallStartLogonOnViewAtStartUp()
        {
            Expect.Call(_view.StartLogon()).Return(true);
            _mocks.ReplayAll();
            _target.Start();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadAvailableSdksOnStepOne()
        {
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_endPointSelector.GetEndpointNames()).Return(new List<string> {"local", "local2"});
            Expect.Call(() => _view.ShowStep(LoginStep.SelectSdk, _model, false));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectSdk;
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGoToDataSourcesIfOneSdk()
        {
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_endPointSelector.GetEndpointNames()).Return(new List<string> { "local" });
            Expect.Call(_dataSourceHandler.DataSourceProviders()).Return(new List<IDataSourceProvider>());
            Expect.Call(() => _view.ShowStep(LoginStep.SelectDatasource, _model, false));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectSdk;
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetDataSources()
        {
            var dataSorceProvider = _mocks.DynamicMock<IDataSourceProvider>();
            var dataSourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
            _model.Sdks = new List<string>{"sdk1", "sdk2"};
            _model.SelectedSdk = "sdk1";
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_dataSourceHandler.DataSourceProviders()).Return(new List<IDataSourceProvider> { dataSorceProvider });
            Expect.Call(dataSorceProvider.DataSourceList()).Return(new List<DataSourceContainer> {dataSourceContainer});
            Expect.Call(() => _view.ShowStep(LoginStep.SelectDatasource, _model, true));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectSdk;
            _target.OkbuttonClicked(_model);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReloadSdkOnBackFromDataSources()
        {
			_model.SelectedDataSourceContainer = new DataSourceContainer(null,null,null, AuthenticationTypeOption.Application);
            Expect.Call(() => _view.ClearForm("")).IgnoreArguments();
            Expect.Call(_endPointSelector.GetEndpointNames()).Return(new List<string> { "local", "local2" });
            Expect.Call(() => _view.ShowStep(LoginStep.SelectSdk, _model, false));
            _mocks.ReplayAll();
            _target.CurrentStep = LoginStep.SelectDatasource;
            _target.BackButtonClicked();
            _mocks.VerifyAll();
        }

        [Test]
        public void LogonModelShouldHaveValidLogin()
        {
            Assert.That(_model.HasValidLogin(), Is.False);
            _model.Password = "p";
            Assert.That(_model.HasValidLogin(), Is.False);
            _model.Password = "";
            _model.UserName = "u";
            Assert.That(_model.HasValidLogin(), Is.False);
            _model.Password = "p";
            Assert.That(_model.HasValidLogin(), Is.True);
        }
	}
}
