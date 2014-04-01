using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.WinCode.Main;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
    public class LogonLicenseCheckerTest
    {
        private MockRepository _mocks;
        private LogonLicenseChecker _target;
        private ILogonView _view;
        private ILicenseStatusLoader _licenseStatusLoader;
        private ILicenseVerifierFactory _licenseVerifierFactory;
        private ILicenseVerifier _licenseVerifier;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.DynamicMock<ILogonView>();
            _licenseStatusLoader = _mocks.DynamicMock<ILicenseStatusLoader>();
            _licenseVerifierFactory = _mocks.DynamicMock<ILicenseVerifierFactory>();
            _licenseVerifier = _mocks.DynamicMock<ILicenseVerifier>();
            _target = new LogonLicenseChecker(_view, _licenseStatusLoader, _licenseVerifierFactory);
        }

        [Test]
        public void ShouldReturnFalseIfNoLicenseService()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var uow = _mocks.DynamicMock<IUnitOfWork>();

            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFactory);
            Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_licenseVerifierFactory.Create(_view, uowFactory)).Return(_licenseVerifier);
            Expect.Call(_licenseVerifier.LoadAndVerifyLicense()).Return(null);
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            Assert.That(_target.HasValidLicense(dataSourceContainer), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfOkAndNotTooMany()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var uow = _mocks.DynamicMock<IUnitOfWork>();
            var licenseService = _mocks.DynamicMock<ILicenseService>();
            var status = _mocks.DynamicMock<ILicenseStatusXml>();
            
            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFactory);
            Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_licenseVerifierFactory.Create(_view, uowFactory)).Return(_licenseVerifier);
            Expect.Call(_licenseVerifier.LoadAndVerifyLicense()).Return(licenseService);
            Expect.Call(_licenseStatusLoader.GetStatus(uow)).Return(status);
            Expect.Call(status.StatusOk).Return(true).Repeat.Times(3);
            Expect.Call(status.AlmostTooMany).Return(false);
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            Assert.That(_target.HasValidLicense(dataSourceContainer), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldWarnAndReturnTrueIfOkAndAlmostTooMany()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var uow = _mocks.DynamicMock<IUnitOfWork>();
            var licenseService = _mocks.DynamicMock<ILicenseService>();
            var status = _mocks.DynamicMock<ILicenseStatusXml>();

            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFactory);
            Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_licenseVerifierFactory.Create(_view, uowFactory)).Return(_licenseVerifier);
            Expect.Call(_licenseVerifier.LoadAndVerifyLicense()).Return(licenseService);
            Expect.Call(_licenseStatusLoader.GetStatus(uow)).Return(status);
            Expect.Call(status.StatusOk).Return(true).Repeat.Times(3);
            Expect.Call(status.AlmostTooMany).Return(true);
            Expect.Call(() => _view.Warning("")).IgnoreArguments();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            Assert.That(_target.HasValidLicense(dataSourceContainer), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldWarnAndReturnTrueIfNotOkAndDaysLeft()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var uow = _mocks.DynamicMock<IUnitOfWork>();
            var licenseService = _mocks.DynamicMock<ILicenseService>();
            var status = _mocks.DynamicMock<ILicenseStatusXml>();

            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFactory);
            Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_licenseVerifierFactory.Create(_view, uowFactory)).Return(_licenseVerifier);
            Expect.Call(_licenseVerifier.LoadAndVerifyLicense()).Return(licenseService);
            Expect.Call(_licenseStatusLoader.GetStatus(uow)).Return(status);
            Expect.Call(status.StatusOk).Return(false).Repeat.Times(3);
            Expect.Call(status.DaysLeft).Return(5);
            Expect.Call(() => _view.Warning("")).IgnoreArguments();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            Assert.That(_target.HasValidLicense(dataSourceContainer), Is.True);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldWarnAndReturnFalseIfNotOkAndNoDaysLeft()
        {
            var dataSourceContainer = _mocks.DynamicMock<IDataSourceContainer>();
            var dataSource = _mocks.DynamicMock<IDataSource>();
            var uowFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            var uow = _mocks.DynamicMock<IUnitOfWork>();
            var licenseService = _mocks.DynamicMock<ILicenseService>();
            var status = _mocks.DynamicMock<ILicenseStatusXml>();

            Expect.Call(dataSourceContainer.DataSource).Return(dataSource);
            Expect.Call(dataSource.Application).Return(uowFactory);
            Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_licenseVerifierFactory.Create(_view, uowFactory)).Return(_licenseVerifier);
            Expect.Call(_licenseVerifier.LoadAndVerifyLicense()).Return(licenseService);
            Expect.Call(_licenseStatusLoader.GetStatus(uow)).Return(status);
            Expect.Call(status.StatusOk).Return(false).Repeat.Times(3);
            Expect.Call(status.DaysLeft).Return(0);
            Expect.Call(() => _view.Error("")).IgnoreArguments();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            Assert.That(_target.HasValidLicense(dataSourceContainer), Is.False);
            _mocks.VerifyAll();
        }
    }
}