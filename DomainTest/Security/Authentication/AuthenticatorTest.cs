using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    /// <summary>
    /// Generic abstract class for Authentication tests
    /// </summary>
    public abstract class AuthenticatorTest<T>
        where T : IAuthenticator
    {
        private MockRepository mocks;
        private T target;
        private IRepositoryFactory repFactory;
        private ILogOnOff _logging;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            repFactory = mocks.CreateMock<IRepositoryFactory>();
            _logging = mocks.CreateMock<ILogOnOff>(); 
            target = CreateAuthenticationService(repFactory, _logging);
        }

        #region Abstract methods

        /// <summary>
        /// Creates the authentication service.
        /// </summary>
        /// <param name="repositoryFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        /// <returns>An concrete AuthenticationServiceOLD</returns>
        protected abstract T CreateAuthenticationService(IRepositoryFactory repositoryFactory, ILogOnOff logging);

        /// <summary>
        /// Finds the user in database.
        /// Implemented by abstract class in suitable way for its authentication
        /// </summary>
        /// <param name="userRep">The user rep.</param>
        /// <param name="user">The user.</param>
        /// <param name="logOnParameter">The login parameter.</param>
        /// <returns><c>true</c> if user is found</returns>
        protected abstract bool FindUserInDatabase(IPersonRepository userRep,
                                                   out IPerson user,
                                                   params object[] logOnParameter);

        /// <summary>
        /// Sets the log on information in a concrete implemented way.
        /// </summary>
        /// <param name="logOnParameter">The login parameter.</param>
        protected abstract void SetLogOnInformation(params object[] logOnParameter);

        #endregion

        #region Protected properties

        /// <summary>
        /// Gets the mock.
        /// </summary>
        /// <value>The mock.</value>
        protected MockRepository Mocks
        {
            get { return mocks; }
        }

        #endregion

        /// <summary>
        /// Verifies that data source user throws exception if user not found.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "foo"), Test]
        [ExpectedException(typeof (PermissionException))]
        public void VerifyDataSourceUserThrowsExceptionIfUserNotFound()
        {
            object foo = target.LoggedOnPerson(mocks.CreateMock<IDataSource>());
        }

        /// <summary>
        /// Verifies that data sources throws exception if data sources notset.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "foo"), Test]
        [ExpectedException(typeof (PermissionException))]
        public void VerifyDataSourcesThrowsExceptionIfDataSourcesNotSet()
        {
            object foo = target.LogonableDataSources;
        }

        /// <summary>
        /// Verifies the log on method works.
        /// </summary>
        [Test]
        public void VerifyLogOnWorks()
        {
            IDictionary<IDataSource, IPerson> possLogins = new Dictionary<IDataSource, IPerson>();
            testAuthSvc authSvc = new testAuthSvc();
            authSvc.SetLogonableDataSourceCollectionForTest(possLogins);
            IDataSource uowFactory = mocks.CreateMock<IDataSource>();
            IPerson user = PersonFactory.CreatePerson("hola3");
            BusinessUnit bu1 = new BusinessUnit("test");
            ((IEntity) bu1).SetId(Guid.NewGuid());
            BusinessUnit bu2 = new BusinessUnit("test2");
            ((IEntity) bu2).SetId(Guid.NewGuid());
            ApplicationRole roleNotChoosenBu = new ApplicationRole();
            typeof(ApplicationRole).GetField("_businessUnit", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(roleNotChoosenBu, bu1);
            ApplicationRole roleChoosenBu = new ApplicationRole();
            typeof(ApplicationRole).GetField("_businessUnit", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(roleChoosenBu, bu2);

            user.PermissionInformation.AddApplicationRole(roleNotChoosenBu);
            user.PermissionInformation.AddApplicationRole(roleChoosenBu);
            possLogins[uowFactory] = user;

        }

        /// <summary>
        /// Verifies the logon fails if business unit is transient.
        /// </summary>
        [Test]
        [ExpectedException(typeof (PermissionException))]
        public void VerifyLogOnFailsIfBusinessUnitIsTransient()
        {
            IDictionary<IDataSource, IPerson> possLogins = new Dictionary<IDataSource, IPerson>();
            testAuthSvc authSvc = new testAuthSvc();
            authSvc.SetLogonableDataSourceCollectionForTest(possLogins);
            IDataSource uowFactory = mocks.CreateMock<IDataSource>();
            IPerson user = PersonFactory.CreatePerson("hola2");
            BusinessUnit bu1 = new BusinessUnit("test");
            ApplicationRole role = new ApplicationRole();
            AvailableData availableData = new AvailableData();
            availableData.AvailableDataRange = AvailableDataRangeOption.MyTeam;
            role.AvailableData = availableData;
            role.SetBusinessUnit(bu1);
            user.PermissionInformation.AddApplicationRole(role);
            possLogins[uowFactory] = user;
            authSvc.LogOn(uowFactory, bu1);
        }

        /// <summary>
        /// Verifies that log on fails if incorrect uow factory.
        /// </summary>
        [Test]
        [ExpectedException(typeof (PermissionException))]
        public void VerifyLogOnFailsIfIncorrectUnitOfWorkFactory()
        {
            IDictionary<IDataSource, IPerson> possLogins = new Dictionary<IDataSource, IPerson>();
            testAuthSvc authSvc = new testAuthSvc();
            authSvc.SetLogonableDataSourceCollectionForTest(possLogins);
            IPerson user = PersonFactory.CreatePerson("hola");
            BusinessUnit buToUse = new BusinessUnit("test");
            ApplicationRole role = new ApplicationRole();
            typeof(ApplicationRole).GetField("_businessUnit", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(role, buToUse);
            user.PermissionInformation.AddApplicationRole(role);
            IDataSource uowfactory = mocks.CreateMock<IDataSource>();
            possLogins[uowfactory] = user;

            authSvc.LogOn(mocks.CreateMock<IDataSource>(), buToUse);
        }

        /// <summary>
        /// Verifies the log on fails if incorrect business unit.
        /// </summary>
        [Test]
        [ExpectedException(typeof (PermissionException))]
        public void VerifyLogOnFailsIfIncorrectBusinessUnit()
        {
            IDictionary<IDataSource, IPerson> possLogins = new Dictionary<IDataSource, IPerson>();
            testAuthSvc authSvc = new testAuthSvc();
            authSvc.SetLogonableDataSourceCollectionForTest(possLogins);
            IPerson user = PersonFactory.CreatePerson("testperson");
            BusinessUnit correctBu = new BusinessUnit("correct");
            BusinessUnit incorrectBu = new BusinessUnit("incorrect");
            ApplicationRole role = new ApplicationRole();
            AvailableData availableData = new AvailableData();
            availableData.AvailableDataRange = AvailableDataRangeOption.MyTeam;
            role.AvailableData = availableData;
            role.SetBusinessUnit(correctBu);
            user.PermissionInformation.AddApplicationRole(role);
            IDataSource uowfactory = mocks.CreateMock<IDataSource>();
            possLogins[uowfactory] = user;

            authSvc.LogOn(uowfactory, incorrectBu);
        }

        private class testAuthSvc : Authenticator
        {
            public void SetLogonableDataSourceCollectionForTest(IDictionary<IDataSource, IPerson> logonableCollection)
            {
                base.SetLogonableDataSourcesExplicitly(logonableCollection);
            }

            /// <summary>
            /// Tries to find user in data base.
            /// This method should be implemented in proper way in concrete class
            /// </summary>
            /// <param name="personRepository">The user rep.</param>
            /// <param name="foundPerson">The found user.</param>
            /// <returns></returns>
            protected override bool TryFindPersonInDataSource(IPersonRepository personRepository, out IPerson foundPerson)
            {
                throw new NotImplementedException();
            }
        }

     

        /// <summary>
        /// Verifies the log off calls clearsession.
        /// </summary>
        [Test]
        public void VerifyLogOffCallsClearSession()
        {
            using (mocks.Record())
            {
                _logging.LogOff();
            }
            using (mocks.Playback())
            {
                target.LogOff();
            }
            mocks.VerifyAll();
        }
    }
}