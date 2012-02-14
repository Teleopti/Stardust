using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    /// <summary>
    /// Tests for ApplicationAuthenticator methods.
    /// </summary>
    [TestFixture]
    public class ApplicationAuthenticationTest : AuthenticatorTest<IApplicationAuthenticator>
    {
        private ApplicationAuthenticator target;

        #region Overriden methods

        /// <summary>
        /// Creates the authentication service.
        /// </summary>
        /// <param name="repositoryFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        /// <returns>An concrete AuthenticationServiceOLD</returns>
        protected override IApplicationAuthenticator CreateAuthenticationService(IRepositoryFactory repositoryFactory, ILogOnOff logging)
        {
            target = new ApplicationAuthenticator(repositoryFactory, logging);
            return target;
        }

        /// <summary>
        /// Finds the user in database.
        /// </summary>
        /// <param name="userRep">The user rep.</param>
        /// <param name="user">The user.</param>
        /// <param name="logOnParameter">The login parameter.</param>
        /// <returns><c>true</c> if user is found</returns>
        protected override bool FindUserInDatabase(IPersonRepository userRep,
                                                   out IPerson user, 
                                                   object[] logOnParameter)
        {
            return userRep.TryFindBasicAuthenticatedPerson(logOnParameter[0].ToString(),
                                                         logOnParameter[1].ToString(),
                                                         out user);
        }

        /// <summary>
        /// Sets the log on information.
        /// </summary>
        /// <param name="logOnParameter">The login parameter.</param>
        protected override void SetLogOnInformation(object[] logOnParameter)
        {
            target.SetLogOnValues(logOnParameter[0].ToString(), logOnParameter[1].ToString());
        }

        #endregion

        #region Tests

        /// <summary>
        /// Verifies that SetLogonValues works.
        /// </summary>
        [Test]
        public void VerifySetLogOnValuesWorks()
        {
            Mocks.ReplayAll();
            string logon = "logon";
            string passw = "password";
            target.SetLogOnValues(logon, passw);
            Assert.AreEqual(logon, target.LogOnName);
            Assert.AreEqual(passw, target.Password);
        }

        /// <summary>
        /// Verifies that ApplicationLogOnName is not set to null.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyLogOnNameIsNotNull()
        {
            Mocks.ReplayAll();
            target.SetLogOnValues(null, string.Empty);
        }

        /// <summary>
        /// Verifies that Password is not set to null.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyPasswordIsNotNull()
        {
            Mocks.ReplayAll();
            target.SetLogOnValues(string.Empty, null);
        }

        /// <summary>
        /// Verifies that null is not passed to constructor.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyNotNullSentToConstructor()
        {
            Mocks.ReplayAll();
            target = new authStub(null, null);
        }

        /// <summary>
        /// Verifies that Logon is not called if logon values has not been set.
        /// </summary>
        /// <remarks>Implement for every concrete class</remarks>
        [Test]
        [ExpectedException(typeof (PermissionException))]
        public void VerifyNotReadingFromDatabaseIfLogOnValuesHasNotBeenSet()
        {
            IUnitOfWork uowMock = Mocks.CreateMock<IUnitOfWork>();
            Mocks.ReplayAll();
            IPerson foundUser;
            authStub testStub = new authStub();
            testStub.TryFindUserInDataBaseTest(new PersonRepository(uowMock), out foundUser);
        }

        [Test]
        public void VerifyApplicationAuthenticationType()
        {
            Assert.IsTrue(AuthenticationTypeOption.Application == target.AuthenticationType);
        }

        private class authStub : ApplicationAuthenticator
        {
            public bool TryFindUserInDataBaseTest(IPersonRepository userRep, out IPerson foundUser)
            {
                return TryFindPersonInDataSource(userRep, out foundUser);
            }

            public authStub(IRepositoryFactory repFactory, ILogOnOff logging) : base(repFactory, logging)
            {
            }

            public authStub()
            {
            }
        }

        #endregion
    }
}