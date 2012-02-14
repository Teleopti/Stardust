using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security
{
    [TestFixture]
    public class UserDetailTest
    {
        private UserDetail target;
        private MockRepository mocks;
        private IPerson person;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            person = mocks.StrictMock<IPerson>();
            target = new UserDetail(person);
        }

        [Test]
        public void VerifyDefaultValues()
        {
            Assert.AreEqual(DateTime.UtcNow.Date,target.LastPasswordChange.Date);
            Assert.AreEqual(0,target.InvalidAttempts);
            Assert.IsFalse(target.IsLocked);
            Assert.AreEqual(DateTime.UtcNow.Date,target.InvalidAttemptsSequenceStart.Date);
            Assert.AreEqual(person,target.Person);
        }

        [Test]
        public void VerifyHasDefaultConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(),true));
        }

        [Test]
        public void VerifyCanLockAccount()
        {
            target.Lock();
            Assert.IsTrue(target.IsLocked);
        }

        [Test]
        public void VerifyCanIncreaseInvalidAttemptCount()
        {
            IPasswordPolicy passwordPolicy = mocks.StrictMock<IPasswordPolicy>();
            using(mocks.Record())
            {
                Expect.Call(passwordPolicy.InvalidAttemptWindow).Return(TimeSpan.FromMinutes(30));
            }
            using (mocks.Playback())
            {
                target.RegisterInvalidAttempt(passwordPolicy);
                Assert.AreEqual(1,target.InvalidAttempts);
            }
        }

        [Test]
        public void VerifyCanResetAfterPasswordChange()
        {
            IPasswordPolicy passwordPolicy = mocks.StrictMock<IPasswordPolicy>();
            using(mocks.Record())
            {
                Expect.Call(passwordPolicy.InvalidAttemptWindow).Return(TimeSpan.FromMinutes(30));
            }
            using (mocks.Playback())
            {
                target.RegisterInvalidAttempt(passwordPolicy);
                target.Lock();
                target.RegisterPasswordChange();
                Assert.AreEqual(0,target.InvalidAttempts);
                Assert.IsFalse(target.IsLocked);
            }
        }
    }
}
