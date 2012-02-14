using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security
{
    [TestFixture]
    public class AuthenticationServiceTest
    {

        #region Variables

        private AuthenticationServiceTestClass _target;
        private MockRepository _mocks;
        private IRepositoryFactory _repFactory;
        private ILogOnOff _logging;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repFactory = _mocks.CreateMock<IRepositoryFactory>();
            _logging = _mocks.CreateMock<ILogOnOff>();
            _target = new AuthenticationServiceTestClass(_repFactory, _logging);
        }

        #endregion

        #region Constructor tests

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyConstructorWithNullRepositoryFactoryParameters()
        {
            _target = new AuthenticationServiceTestClass(null, _logging);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyConstructorWithNullLoggingParameters()
        {
            _target = new AuthenticationServiceTestClass(_repFactory, null);
        }

        #endregion

        #region Method tests

        [Test]
        public void VerifyAuthenticationType()
        {
            // expected Unknown as default
            AuthenticationTypeOption expected = AuthenticationTypeOption.Unknown;

            Assert.AreEqual(expected, _target.AuthenticationType);

            expected = AuthenticationTypeOption.Windows;
            _target.AuthenticationType = expected;
            Assert.AreEqual(expected, _target.AuthenticationType);

        }

        [Test]
        public void VerifyAuthenticator()
        {
            IAuthenticator authenticator = _mocks.CreateMock<IAuthenticator>();
            IApplicationAuthenticator application = _mocks.CreateMock<IApplicationAuthenticator>();
            IWindowsAuthenticator windows = _mocks.CreateMock<IWindowsAuthenticator>();

            _target.Authenticator = authenticator;
            IAuthenticator expected = authenticator;
            Assert.AreSame(expected, _target.Authenticator);

            _target.WindowsAuthenticator = windows;
            _target.ApplicationAuthenticator = application;

            // now the ApplicationAuthenticator is the last used
            expected = application;
            Assert.AreSame(expected, _target.Authenticator);

            // set authentication type

            _target.AuthenticationType = AuthenticationTypeOption.Windows;
            expected = windows;
            Assert.AreSame(expected, _target.Authenticator);

            _target.AuthenticationType = AuthenticationTypeOption.Application;
            expected = application;
            Assert.AreSame(expected, _target.Authenticator);
        }

        [Test]
        public void VerifyCreateLogonableDataSourcesListForWindowsUser()
        {
            string domainName = "domainName";
            string userName = "userName";
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IWindowsAuthenticator authenticator = _mocks.CreateMock<IWindowsAuthenticator>();
            _target.WindowsAuthenticator = authenticator;

            authenticator.SetLogOnValues(domainName, userName);
            LastCall.Repeat.Once();
            authenticator.DefineDataSources();
            LastCall.Repeat.Once();

            Expect.Call(authenticator.LogonableDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> {datasource}));

            _mocks.ReplayAll();

            IList<IDataSource> result = _target.CreateLogonableDataSourcesListForWindowsUser(domainName, userName);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(datasource, result[0]);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyCreateAvailableDataSourcesListForApplicationUser()
        {
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IWindowsAuthenticator authenticator = _mocks.CreateMock<IWindowsAuthenticator>();
            _target.WindowsAuthenticator = authenticator;

            Expect.Call(authenticator.AvailableDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> { datasource }));

            _mocks.ReplayAll();

            IList<IDataSource> result = _target.CreateAvailableDataSourcesListForApplicationUser();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(datasource, result[0]);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCreateAvailableBusinessUnitCollection()
        {
            string dataSourceName = "DataSourceName";
            string businessUnitName = "BusinessUnitName";
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IUnitOfWorkFactory application = _mocks.CreateMock<IUnitOfWorkFactory>();
            IAuthenticator authenticator = _mocks.CreateMock<IAuthenticator>();
            IBusinessUnit businessUnit = _mocks.CreateMock<IBusinessUnit>();
            _target.Authenticator = authenticator;

            _mocks.Record();

            Expect.Call(datasource.Application).Return(application).Repeat.Once();
            Expect.Call(application.Name).Return(dataSourceName);
            Expect.Call(authenticator.LogonableDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> { datasource }));
            Expect.Call(authenticator.AvailableBusinessUnits(datasource)).Return(new List<IBusinessUnit> { businessUnit }).Repeat.Once();
            Expect.Call(businessUnit.Name).Return(businessUnitName).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            IList<IBusinessUnit> result = _target.CreateAvailableBusinessUnitCollection(dataSourceName);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(businessUnit.Name, result[0].Name);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCreateAvailableBusinessUnitCollectionWithNullResult()
        {
            string dataSourceName = "DataSourceName";
            string notFoundDataSourceName = "NotFoundDataSourceName";
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IUnitOfWorkFactory application = _mocks.CreateMock<IUnitOfWorkFactory>();
            IAuthenticator authenticator = _mocks.CreateMock<IAuthenticator>();
            _target.Authenticator = authenticator;

            _mocks.Record();

            Expect.Call(datasource.Application).Return(application).Repeat.Once();
            Expect.Call(application.Name).Return(dataSourceName);
            Expect.Call(authenticator.LogonableDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> { datasource }));

            _mocks.ReplayAll();

            Assert.IsNull(_target.CreateAvailableBusinessUnitCollection(notFoundDataSourceName));

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCreateAvailableBusinessUnitCollectionNonExistingDataSourceName()
        {
            string dataSourceName = "DataSourceName";
            string nonExistingdataSourceName = "NonExistingDataSourceName";
            //string businessUnitName = "BusinessUnitName";
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IUnitOfWorkFactory application = _mocks.CreateMock<IUnitOfWorkFactory>();
            IAuthenticator authenticator = _mocks.CreateMock<IAuthenticator>();
            //IBusinessUnit businessUnit = _mocks.CreateMock<IBusinessUnit>();
            _target.Authenticator = authenticator;

            _mocks.Record();

            Expect.Call(datasource.Application).Return(application).Repeat.Once();
            Expect.Call(application.Name).Return(dataSourceName);
            //Expect.Call(businessUnit.Name).Return(businessUnitName).Repeat.AtLeastOnce();
            Expect.Call(authenticator.LogonableDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> { datasource }));

            _mocks.ReplayAll();

            IList<IBusinessUnit> result = _target.CreateAvailableBusinessUnitCollection(nonExistingdataSourceName);

            Assert.IsNull(result);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCheckDataSourceIsLogonableForApplicationUser()
        {
            string dataSourceName = "DataSourceName";
            string logOnName = "AcdLogOnName";
            string password = "Password";
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IUnitOfWorkFactory application = _mocks.CreateMock<IUnitOfWorkFactory>();
            IApplicationAuthenticator authenticator = _mocks.CreateMock<IApplicationAuthenticator>();
            _target.ApplicationAuthenticator = authenticator;

            _mocks.Record();

            authenticator.SetLogOnValues(logOnName, password);
            LastCall.Repeat.Once();

            Expect.Call(datasource.Application).Return(application).Repeat.Once();
            Expect.Call(application.Name).Return(dataSourceName);
            Expect.Call(authenticator.RegisteredDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> { datasource }));
            Expect.Call(authenticator.DefineDataSourceIsLogonable(datasource)).Return(true).Repeat.Once();

            _mocks.ReplayAll();

            bool result = _target.CheckDataSourceIsLogonableForApplicationUser(dataSourceName, logOnName, password);

            Assert.IsTrue(result);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCheckDataSourceIsLogonableForApplicationUserWithNonExistingDataSourceName()
        {
            string dataSourceName = "DataSourceName";
            string nonExistingdataSourceName = "NonExistingDataSourceName";
            string logOnName = "AcdLogOnName";
            string password = "Password";
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IUnitOfWorkFactory application = _mocks.CreateMock<IUnitOfWorkFactory>();
            IApplicationAuthenticator authenticator = _mocks.CreateMock<IApplicationAuthenticator>();
            _target.ApplicationAuthenticator = authenticator;

            _mocks.Record();

            Expect.Call(datasource.Application).Return(application).Repeat.Once();
            Expect.Call(application.Name).Return(dataSourceName);
            Expect.Call(authenticator.RegisteredDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> { datasource }));

            _mocks.ReplayAll();

            bool result = _target.CheckDataSourceIsLogonableForApplicationUser(nonExistingdataSourceName, logOnName, password);

            Assert.IsFalse(result);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyLogOn()
        {
            string dataSourceName = "DataSourceName";
            IDataSource datasource = _mocks.CreateMock<IDataSource>();
            IUnitOfWorkFactory application = _mocks.CreateMock<IUnitOfWorkFactory>();
            IBusinessUnit businessUnit = _mocks.CreateMock<IBusinessUnit>();

            IAuthenticator authenticator = _mocks.CreateMock<IAuthenticator>();
            _target.Authenticator = authenticator;

            _mocks.Record();

            Expect.Call(datasource.Application).Return(application).Repeat.Once();
            Expect.Call(application.Name).Return(dataSourceName);
            Expect.Call(authenticator.LogonableDataSources).Return(
                new ReadOnlyCollection<IDataSource>(new List<IDataSource> { datasource }));

            authenticator.LogOn(datasource, businessUnit);
            LastCall.Repeat.Once();

            _mocks.ReplayAll();

            _target.LogOn(dataSourceName, businessUnit);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyLogOff()
        {

            IAuthenticator authenticator = _mocks.CreateMock<IAuthenticator>();
            _target.Authenticator = authenticator;

            _mocks.Record();

            authenticator.LogOff();
            LastCall.Repeat.Once();

            _mocks.ReplayAll();

            _target.LogOff();

            _mocks.VerifyAll();
        }

        #endregion

        #region Local Test methods

        [Test]
        public void VerifyWindowsAuthenticator()
        {
            _target.WindowsAuthenticator = null;
            IWindowsAuthenticator result = _target.WindowsAuthenticator;
            Assert.IsNotNull(result);

            IWindowsAuthenticator windowsAuthenticator = _mocks.CreateMock<IWindowsAuthenticator>();
            _target.WindowsAuthenticator = windowsAuthenticator;
            result = _target.WindowsAuthenticator;
            Assert.AreSame(windowsAuthenticator, result);

        }

        [Test]
        public void VerifyApplicationAuthenticator()
        {
            _target.ApplicationAuthenticator = null;
            IApplicationAuthenticator result = _target.ApplicationAuthenticator;
            Assert.IsNotNull(result);

            IApplicationAuthenticator applicationAuthenticator = _mocks.CreateMock<IApplicationAuthenticator>();
            _target.ApplicationAuthenticator = applicationAuthenticator;
            result = _target.ApplicationAuthenticator;
            Assert.AreSame(applicationAuthenticator, result);

        }

        #endregion

    }
}
