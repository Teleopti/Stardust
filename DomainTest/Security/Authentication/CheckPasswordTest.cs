using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class CheckPasswordTest
    {
        private MockRepository mocks;
        private ICheckPassword target;
        private IOneWayEncryption encryption;
        private IApplicationAuthenticationInfo applicationAuthenticationInfo;
        private IPerson person;
        private IUserDetail userDetail;
        private ICheckBruteForce checkBruteForce;
        private ICheckPasswordChange checkPasswordChange;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            encryption = mocks.StrictMock<IOneWayEncryption>();
            userDetail = mocks.StrictMock<IUserDetail>();
            person = mocks.StrictMock<IPerson>();
            applicationAuthenticationInfo =
                mocks.StrictMock<IApplicationAuthenticationInfo>();
            checkBruteForce = mocks.StrictMock<ICheckBruteForce>();
            checkPasswordChange = mocks.StrictMock<ICheckPasswordChange>();
            target = new CheckPassword(encryption,checkBruteForce,checkPasswordChange);
        }

        [Test]
        public void VerifyCheckPasswordOk()
        {
            const string password = "pass";
            var authenticationResult = new AuthenticationResult();
            using(mocks.Record())
            {
                Expect.Call(encryption.EncryptString(password)).Return(password);
                Expect.Call(userDetail.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(person.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo);
                Expect.Call(applicationAuthenticationInfo.Password).Return(password);
                Expect.Call(checkPasswordChange.Check(userDetail)).Return(authenticationResult);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(userDetail, password);
                Assert.AreEqual(authenticationResult,result);
            }
        }

        [Test]
        public void VerifyCheckInvalidPassword()
        {
            const string password = "pass";
            using (mocks.Record())
            {
                Expect.Call(encryption.EncryptString(password)).Return(password);
                Expect.Call(userDetail.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(person.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo);
                Expect.Call(applicationAuthenticationInfo.Password).Return("invalidpass");
                Expect.Call(checkBruteForce.Check(userDetail)).Return(new AuthenticationResult());
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(userDetail, password);
                Assert.IsFalse(result.Successful);
                Assert.IsTrue(result.HasMessage);
                Assert.AreEqual(UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword, result.Message);
            }
        }
    }
}
