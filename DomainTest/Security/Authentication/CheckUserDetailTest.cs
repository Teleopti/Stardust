using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class CheckUserDetailTest
    {
        private MockRepository mocks;
        private ICheckPassword checkPassword;
        private ICheckUserDetail target;
        private IUserDetail userDetail;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            checkPassword = mocks.StrictMock<ICheckPassword>();
            userDetail = mocks.StrictMock<IUserDetail>();
            target = new CheckUserDetail(checkPassword);
        }

        [Test]
        public void VerifyCheckUserDetailWithLockedUser()
        {
            const string password = "pass";
            var person = mocks.StrictMock<IPerson>();
            using (mocks.Record())
            {
                Expect.Call(userDetail.IsLocked).Return(true);
                Expect.Call(userDetail.Person).Return(person);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(userDetail, password);
                Assert.IsFalse(result.Successful);
                Assert.IsTrue(result.HasMessage);
                Assert.AreEqual(UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword, result.Message);
            }
        }

        [Test]
        public void VerifyCheckUserDetail()
        {
            const string password = "pass";
            var authenticationResult = new AuthenticationResult();
            using (mocks.Record())
            {
                Expect.Call(checkPassword.CheckLogOn(userDetail, password)).Return(authenticationResult);
                Expect.Call(userDetail.IsLocked).Return(false);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(userDetail, password);
                Assert.AreEqual(authenticationResult, result);
            }
        }
    }
}
