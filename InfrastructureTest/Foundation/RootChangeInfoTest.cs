using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class RootChangeInfoTest
    {
        private MockRepository mocks;
        private IAggregateRoot root;
        private DomainUpdateType status;
        private RootChangeInfo target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            root = new Person();
            root.SetId(Guid.NewGuid());
            status = DomainUpdateType.Insert;
            target = new RootChangeInfo(root, status);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(root, target.Root);
            Assert.AreEqual(status, target.Status);
        }

        #region Equals, operators and GetHashCode tests

        [Test]
        public void VerifyEqualsWork()
        {
            Assert.AreEqual(target, new RootChangeInfo(root, DomainUpdateType.Insert));
            Assert.AreNotEqual(target, new RootChangeInfo(root, DomainUpdateType.Update));
            Assert.AreNotEqual(target, new RootChangeInfo(mocks.StrictMock<IAggregateRoot>(), DomainUpdateType.Insert));
            Assert.IsFalse(target.Equals(null));
        }

        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            Assert.IsTrue(target == new RootChangeInfo(root, status));
            Assert.IsTrue(target != new RootChangeInfo(root, DomainUpdateType.Delete));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            Assert.AreEqual(target.GetHashCode(), new RootChangeInfo(root, status).GetHashCode());
        }

        #endregion
    }
}
