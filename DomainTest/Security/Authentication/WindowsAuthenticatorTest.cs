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
    /// Tests for WindowsAuthenticator
    /// </summary>
    [TestFixture]
    public class WindowsAuthenticatorTest : AuthenticatorTest<IWindowsAuthenticator>
    {
        private WindowsAuthenticator target;

        #region Overriden methods

        /// <summary>
        /// Creates the authentication service.
        /// </summary>
        /// <param name="repositoryFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        /// <returns>An concrete AuthenticationService</returns>
        protected override IWindowsAuthenticator CreateAuthenticationService(IRepositoryFactory repositoryFactory, ILogOnOff logging)
        {
            target = new WindowsAuthenticator(repositoryFactory, logging);
            return target;
        }

        /// <summary>
        /// Finds the user in database.
        /// Implemented by abstract class in suitable way for its authentication
        /// </summary>
        /// <param name="userRep">The user rep.</param>
        /// <param name="user">The user.</param>
        /// <param name="logOnParameter">The login parameter.</param>
        /// <returns><c>true</c> if user is found</returns>
        protected override bool FindUserInDatabase(IPersonRepository userRep, out IPerson user, object[] logOnParameter)
        {
            return
                userRep.TryFindWindowsAuthenticatedPerson(logOnParameter[0].ToString(), logOnParameter[1].ToString(),
                                                        out user);
        }

        /// <summary>
        /// Sets the log on information in a concrete implemented way.
        /// </summary>
        /// <param name="logOnParameter">The login parameter.</param>
        protected override void SetLogOnInformation(object[] logOnParameter)
        {
            target.SetLogOnValues(logOnParameter[0].ToString(), logOnParameter[1].ToString());
        }

        #endregion

        /// <summary>
        /// Verifies that SetLogonValues works.
        /// </summary>
        [Test]
        public void VerifySetLogOnValuesWorks()
        {
            string dName = "domain";
            string uName = "User name";
            target.SetLogOnValues(dName, uName);
            Assert.AreEqual(dName, target.DomainName);
            Assert.AreEqual(uName, target.LogOnName);
        }

        /// <summary>
        /// Verifies that domain name not set to null.
        /// </summary>
        /// <remarks>
        /// rk: maybe this should be allowed if domains are not used?
        /// </remarks>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyDomainNameNotSetToNull()
        {
            target.SetLogOnValues(null, "logOnName");
        }

        /// <summary>
        /// Verifies that logonname is not null.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyLogOnNameNotNull()
        {
            target.SetLogOnValues("fdsfds", null);
        }

        /// <summary>
        /// Verifies that null is not passed to constructor.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyNotNullSentToConstructor()
        {
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
            Mocks.VerifyAll();
        }

        [Test]
        public void VerifyWindowsAuthenticationType()
        {
            Assert.IsTrue(AuthenticationTypeOption.Windows == target.AuthenticationType);
        }

        private class authStub : WindowsAuthenticator
        {
            public void TryFindUserInDataBaseTest(IPersonRepository userRep, out IPerson foundUser)
            {
                TryFindPersonInDataSource(userRep, out foundUser);
            }

            public authStub(IRepositoryFactory repFactory, ILogOnOff logging) : base(repFactory, logging)
            {
            }

            public authStub()
            {
            }
        }
    }
}