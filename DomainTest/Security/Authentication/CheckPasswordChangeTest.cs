using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class CheckPasswordChangeTest
    {
        private MockRepository mocks;
        private ICheckPasswordChange target;
        private IUserDetail userDetail;
        private IPasswordPolicy passwordPolicy;
        private IPerson person;

	    [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            passwordPolicy = mocks.StrictMock<IPasswordPolicy>();
            userDetail = mocks.StrictMock<IUserDetail>();
            person = mocks.StrictMock<IPerson>();
		    target = new CheckPasswordChange(() => passwordPolicy);
        }

        [Test]
        public void VerifyPasswordValidForDayCountCanBeMax()
        {
            //by default, its int.max just check that it passes
            using (mocks.Record())
            {
                Expect.Call(userDetail.Person).Return(person);
                Expect.Call(userDetail.LastPasswordChange).Return(DateTime.Today.AddDays(-15));
                Expect.Call(passwordPolicy.PasswordValidForDayCount).Return(int.MaxValue);
                Expect.Call(passwordPolicy.PasswordExpireWarningDayCount).Return(6);

            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.Check(userDetail);
                Assert.IsTrue(result.Successful);
                Assert.IsNotNull(result);
               
            }

        }

        [Test]
        public void VerifyPasswordShouldHaveBeenChanged()
        {
            using(mocks.Record())
            {
                Expect.Call(userDetail.Person).Return(person);
                Expect.Call(userDetail.LastPasswordChange).Return(DateTime.Today.AddDays(-15));
                Expect.Call(passwordPolicy.PasswordValidForDayCount).Return(10);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.Check(userDetail);
                Assert.IsFalse(result.Successful);
                Assert.IsTrue(result.HasMessage);
				Assert.IsTrue(result.PasswordExpired);
                Assert.AreEqual(UserTexts.Resources.LogOnFailedPasswordExpired, result.Message);
            }
        }

        [Test]
        public void VerifyPasswordMustSoonBeChanged()
        {
            using (mocks.Record())
            {
                Expect.Call(userDetail.Person).Return(person);
                Expect.Call(userDetail.LastPasswordChange).Return(DateTime.Today.AddDays(-5));
                Expect.Call(passwordPolicy.PasswordValidForDayCount).Return(10);
                Expect.Call(passwordPolicy.PasswordExpireWarningDayCount).Return(6);
            }
            using (mocks.Playback())
            {
                AuthenticationResult result = target.Check(userDetail);
                Assert.IsTrue(result.Successful);
	            Assert.IsTrue(result.HasMessage);
                Assert.IsFalse(string.IsNullOrEmpty(result.Message));
            }
        }
    }
}
