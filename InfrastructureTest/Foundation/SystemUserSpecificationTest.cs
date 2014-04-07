using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    public class SystemUserSpecificationTest
    {
        private ISpecification<IPerson> target;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            target = new SystemUserSpecification();
        }

        [Test]
        public void VerifySystemUser()
        {
            var person = mocks.StrictMock<IPerson>();
            var applicationAuthenticationInfo = mocks.StrictMock<IApplicationAuthenticationInfo>();
            using (mocks.Record())
            {
                Expect.Call(person.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo).Repeat.Times(2);
                Expect.Call(applicationAuthenticationInfo.ApplicationLogOnName).Return(SuperUser.UserName);
            }
            using (mocks.Playback())
            {
                Assert.IsTrue(target.IsSatisfiedBy(person));
            }
        }

        [Test]
        public void VerifyNonSystemUser()
        {
            var person = mocks.StrictMock<IPerson>();
            var applicationAuthenticationInfo = mocks.StrictMock<IApplicationAuthenticationInfo>();
            using (mocks.Record())
            {
                Expect.Call(person.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo).Repeat.Times(2);
                Expect.Call(applicationAuthenticationInfo.ApplicationLogOnName).Return("KalleK");
            }
            using (mocks.Playback())
            {
                Assert.IsFalse(target.IsSatisfiedBy(person));
            }
        }

        [Test]
        public void VerifyNullUser()
        {
            Assert.IsFalse(target.IsSatisfiedBy(null));
        }
    }
}
