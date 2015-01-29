using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class CheckNullUserTest
    {
        private MockRepository mocks;
        private IFindUserDetail findUserDetail;
        private ICheckNullUser target;
        private IUnitOfWork unitOfWork;
        
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            findUserDetail = mocks.StrictMock<IFindUserDetail>();
            unitOfWork = mocks.StrictMock<IUnitOfWork>();
            target = new CheckNullUser(findUserDetail);
        }

        [Test]
        public void VerifyCheckNullLogOn()
        {
            const string password = "pass";
            const IPerson person = null;
            AuthenticationResult result = target.CheckLogOn(unitOfWork, person, password);
            Assert.IsFalse(result.Successful);
            Assert.IsTrue(result.HasMessage);
            Assert.AreEqual(UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword, result.Message);
        }

        [Test]
        public void VerifyCheckNotNullLogOn()
        {
            const string password = "pass";
            IPerson person = mocks.StrictMock<IPerson>();
            var authenticationResult = new AuthenticationResult();
            using (mocks.Record())
            {
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
