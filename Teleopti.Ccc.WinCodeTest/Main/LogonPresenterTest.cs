using NUnit.Framework;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.WinCode.Main;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
            _target = new LogonPresenter(_view, _model, _dataSourceHandler, _initializer, _logonLogger, _logOnOff);
        }

        [Test]
        public void ShouldCallStartLogonOnViewAtStartUp()
        {
            Expect.Call(() => _view.StartLogon());
            _mocks.ReplayAll();
            _target.Start();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadAvailableSdksOnStepOne()
        {
            
        }
	}
}
