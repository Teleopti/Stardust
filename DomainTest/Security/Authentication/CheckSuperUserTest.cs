using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class CheckSuperUserTest
    {
        private MockRepository mocks;
        private ICheckSuperUser target;
        private IFindUserDetail findUserDetail;
        private IUnitOfWork unitOfWork;
        private ISystemUserSpecification systemUserSpecification;
        private ISystemUserPasswordSpecification systemUserPasswordSpecification;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            findUserDetail = mocks.StrictMock<IFindUserDetail>();
            unitOfWork = mocks.StrictMock<IUnitOfWork>();
            systemUserSpecification = mocks.StrictMock<ISystemUserSpecification>();
            systemUserPasswordSpecification = mocks.StrictMock<ISystemUserPasswordSpecification>();
            target = new CheckSuperUser(findUserDetail, systemUserSpecification, systemUserPasswordSpecification);
        }

        [Test]
        public void VerifyCheckSuperUserLogOn()
        {
            const string password = "pass";
            IPerson person = mocks.StrictMock<IPerson>();
            using(mocks.Record())
            {
                Expect.Call(systemUserSpecification.IsSatisfiedBy(person)).Return(true);
                Expect.Call(systemUserPasswordSpecification.IsSatisfiedBy(password)).Return(true);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(unitOfWork, person, password);
                Assert.IsTrue(result.Successful);
                Assert.IsFalse(result.HasMessage);
            }
        }

        [Test]
        public void VerifyCheckSuperUserPassword()
        {
            const string password = "pass";
            IPerson person = mocks.StrictMock<IPerson>();
            var authenticationResult = new AuthenticationResult();
            using (mocks.Record())
            {
                Expect.Call(systemUserSpecification.IsSatisfiedBy(person)).Return(true);
                Expect.Call(systemUserPasswordSpecification.IsSatisfiedBy(password)).Return(false);
                Expect.Call(findUserDetail.CheckLogOn(unitOfWork, person, password)).Return(authenticationResult);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(unitOfWork, person, password);
                Assert.AreEqual(authenticationResult, result);
            }
        }

        [Test]
        public void VerifyCheckNonSuperUserLogOn()
        {
            const string password = "pass";
            IPerson person = mocks.StrictMock<IPerson>();
            var authenticationResult = new AuthenticationResult();
            using (mocks.Record())
            {
                Expect.Call(systemUserSpecification.IsSatisfiedBy(person)).Return(false);
                Expect.Call(findUserDetail.CheckLogOn(unitOfWork, person, password)).Return(authenticationResult);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(unitOfWork, person, password);
                Assert.AreEqual(authenticationResult, result);
            }
        }
    }
}
