using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for Entity
    /// </summary>
    [TestFixture]
    public class EntityTest
    {
        private Entity target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new EntityDummy();
        }

        /// <summary>
        /// Determines whether this instance can set its id.
        /// </summary>
        [Test]
        public void CanSetId()
        {
            Guid newId = Guid.NewGuid();
            target = new EntityDummy();
            ((IEntity) target).SetId(newId);
            Assert.AreEqual(newId, target.Id);
        }

        /// <summary>
        /// Verifies the equals implementation.
        /// </summary>
        [Test]
        public void VerifyEquals()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            IEntity nullId = new EntityDummy();
            IEntity nullId2 = new EntityDummy();
            IEntity Id1 = new Person();
            Id1.SetId(guid1);
            Person Id1Copy = new Person();
            ((IEntity) Id1Copy).SetId(guid1);
            IEntity Id2 = new EntityDummy();
            Id2.SetId(guid2);

            Assert.IsFalse(nullId.Equals(nullId2));
            Assert.IsTrue(Id1.Equals(Id1Copy));
            Assert.IsFalse(Id1Copy.Equals(Id2));
            Assert.IsFalse(nullId.Equals(null));
            Assert.IsFalse(Id1.Equals(nullId2));
            Assert.IsFalse(Id1.Equals(3));
            Assert.IsTrue(nullId.Equals(nullId));
        }

        /// <summary>
        /// Verifies GetHashCode.
        /// </summary>
        [Test]
        public void VerifyGetHashCode()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            IEntity nullId = new EntityDummy();
            IEntity nullId2 = new EntityDummy();
            IEntity Id1 = new Person();
            Id1.SetId(guid1);
            Person Id1Copy = new Person();
            ((IEntity) Id1Copy).SetId(guid1);
            IEntity Id2 = new EntityDummy();
            Id2.SetId(guid2);

            Assert.AreNotEqual(nullId.GetHashCode(), nullId2.GetHashCode());
            Assert.AreEqual(Id1.GetHashCode(), Id1Copy.GetHashCode());
            Assert.AreNotEqual(Id1.GetHashCode(), Id2.GetHashCode());
            Assert.AreEqual(Id1.GetHashCode(), Id1.GetHashCode());
        }

        /// <summary>
        /// Verifies ToString().
        /// </summary>
        [Test]
        public void VerifyToString()
        {
            target = new EntityDummy();
            Assert.AreEqual("EntityDummy, no id", target.ToString());
            Guid id = Guid.NewGuid();
            ((IEntity) target).SetId(id);
            Assert.AreEqual("EntityDummy, " + id, target.ToString());
        }

        [Test]
        public void VerifyHashCodeIsKept()
        {
            IDictionary<IPerson, int> dic = new Dictionary<IPerson, int>();
            Person per = new Person();
            dic[per] = 3;
            Assert.IsTrue(dic.ContainsKey(per));

            //simulate persist
            ((IEntity)per).SetId(Guid.NewGuid());
            
            Assert.IsTrue(dic.ContainsKey(per));
        }

        private class EntityDummy : Entity
        {
        }
    }
}