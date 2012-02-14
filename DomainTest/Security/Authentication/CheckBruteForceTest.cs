using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class CheckBruteForceTest
    {
        private MockRepository mocks;
        private ICheckBruteForce target;
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
            target = new CheckBruteForce(passwordPolicy);
        }

        [Test]
        public void VerifyBruteForceIsRegistered()
        {
            using(mocks.Record())
            {
                Expect.Call(userDetail.Person).Return(person);
                Expect.Call(()=> userDetail.RegisterInvalidAttempt(passwordPolicy));
                Expect.Call(userDetail.InvalidAttempts).Return(1);
                Expect.Call(passwordPolicy.MaxAttemptCount).Return(3);
            }
            using (mocks.Playback())
            {
                var result = target.Check(userDetail);
                Assert.IsFalse(result.Successful);
                Assert.IsFalse(result.HasMessage);
                Assert.AreEqual(person,result.Person);
            }
        }

        [Test]
        public void VerifyTooManyBruteForceAttemptsLocksUser()
        {
            using (mocks.Record())
            {
                Expect.Call(userDetail.Person).Return(person);
                Expect.Call(() => userDetail.RegisterInvalidAttempt(passwordPolicy));
                Expect.Call(userDetail.InvalidAttempts).Return(4);
                Expect.Call(passwordPolicy.MaxAttemptCount).Return(3);
                Expect.Call(userDetail.Lock);
            }
            using (mocks.Playback())
            {
                var result = target.Check(userDetail);
                Assert.IsFalse(result.Successful);
                Assert.IsTrue(result.HasMessage);
                Assert.AreEqual(person, result.Person);
            }
        }
    }
}
