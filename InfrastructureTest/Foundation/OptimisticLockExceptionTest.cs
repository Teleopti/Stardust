using System;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("BucketB")]
    public class OptimisticLockExceptionTest : ExceptionTest<OptimisticLockException>
    {
        [Test]
        public void VerifyConstructorWithInfo()
        {
            const string message = "for test";
            const string entityName = "test type";
            Guid id = Guid.NewGuid();
            StaleObjectStateException inner = new StaleObjectStateException(entityName, id);
            OptimisticLockException ex = new OptimisticLockException(message, inner);
            StringAssert.Contains(message, ex.Message);
            Assert.AreSame(inner, ex.InnerException);
            Assert.AreEqual(entityName, ex.EntityName);
            Assert.AreEqual(id, ex.RootId);
            Assert.IsTrue(ex.HasEntityInformation);
        }

        [Test]
        public void VerifyConstructorWithoutInfo()
        {
            MockRepository mocks = new MockRepository();
            Exception inner = mocks.StrictMock<StaleStateException>("hejhej");
            const string message = "for test";
            OptimisticLockException ex = new OptimisticLockException(message, inner);
			StringAssert.Contains(message, ex.Message);
			Assert.AreSame(inner, ex.InnerException);
            Assert.AreEqual(string.Empty, ex.EntityName);
            Assert.AreEqual(Guid.Empty, ex.RootId);
            Assert.IsFalse(ex.HasEntityInformation);
        }

        protected override OptimisticLockException CreateTestInstance(string message, Exception innerException)
        {
            return new OptimisticLockException(message, innerException);
        }

        protected override OptimisticLockException CreateTestInstance(string message)
        {
            return new OptimisticLockException(message);
        }


        [Test]
        public override void VerifyProtectedConstructorWorks()
        {
            string entityName = "test type";
            Guid id = Guid.NewGuid();
            StaleObjectStateException inner = new StaleObjectStateException(entityName, id);
            string message = "for test";
            OptimisticLockException exOrg = new OptimisticLockException(message, inner);
            var result = SerializationHelper.SerializeAsBinary(exOrg);
            OptimisticLockException serialized = SerializationHelper.Deserialize<OptimisticLockException>(result);
            Assert.IsNotNull(serialized);
			StringAssert.Contains(message, serialized.Message);
			Assert.AreEqual(inner.Message, serialized.InnerException.Message);
            Assert.AreEqual(entityName, serialized.EntityName);
            Assert.AreEqual(id, serialized.RootId);
        }
    }
}
