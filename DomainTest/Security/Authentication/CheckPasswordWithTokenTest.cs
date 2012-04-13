using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class CheckPasswordWithTokenTest
    {
        private MockRepository mocks;
        private ICheckPassword target;
        private IOneWayEncryption encryption;
        private IApplicationAuthenticationInfo applicationAuthenticationInfo;
        private IPerson person;
        private IUserDetail userDetail;
        private ICheckBruteForce checkBruteForce;
        private ICheckPasswordChange checkPasswordChange;
    	private IPassphraseProvider passphraseProvider;

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
            passphraseProvider = mocks.StrictMock<IPassphraseProvider>();
            target = new CheckPasswordWithToken(encryption,passphraseProvider,checkBruteForce,checkPasswordChange);
        }

        [Test]
        public void VerifyCheckPasswordOk()
        {
            const string password = "pass";
            const string logOn = "nisse";
        	const string passphrase = "auth";

            using(mocks.Record())
            {
                Expect.Call(encryption.EncryptStringWithBase64(passphrase, logOn)).Return(password);
                Expect.Call(userDetail.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(person.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo);
                Expect.Call(applicationAuthenticationInfo.ApplicationLogOnName).Return(logOn);
                Expect.Call(passphraseProvider.Passphrase()).Return(passphrase);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.CheckLogOn(userDetail, password);
                result.Successful.Should().Be.True();
            }
        }

        [Test]
        public void VerifyCheckInvalidPassword()
        {
			const string password = "pass";
			const string logOn = "nisse";
			const string passphrase = "auth";

            using (mocks.Record())
            {
                Expect.Call(encryption.EncryptString(password)).Return(password);
                Expect.Call(encryption.EncryptStringWithBase64(passphrase, logOn)).Return("invalidauthpass");
                Expect.Call(userDetail.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(person.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo).Repeat.Twice();
                Expect.Call(applicationAuthenticationInfo.Password).Return("invalidpass");
				Expect.Call(applicationAuthenticationInfo.ApplicationLogOnName).Return(logOn);
				Expect.Call(passphraseProvider.Passphrase()).Return(passphrase);
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
