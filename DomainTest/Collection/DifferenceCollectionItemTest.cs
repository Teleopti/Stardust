using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class DifferenceCollectionItemTest
    {
        private DifferenceCollectionItem<Person> target;
        private Person org;
        private Person curr;

        [SetUp]
        public void Setup()
        {
            org = new Person();
            curr = new Person();
            target = new DifferenceCollectionItem<Person>(org, curr);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(org, target.OriginalItem);
            Assert.AreSame(curr, target.CurrentItem);
        }

        [Test]
        public void VerifyStatus()
        {
            Assert.AreEqual(DifferenceStatus.Added, new DifferenceCollectionItem<Person>(null, new Person()).Status);
            Assert.AreEqual(DifferenceStatus.Deleted, new DifferenceCollectionItem<Person>(new Person(), null).Status);
            Assert.AreEqual(DifferenceStatus.Modified, new DifferenceCollectionItem<Person>(new Person(), new Person()).Status);
        }


        [Test]
        public void VerifyOperatorOverloading()
        {
            Assert.IsTrue(target == new DifferenceCollectionItem<Person>(org, curr));
            Assert.IsTrue(target != new DifferenceCollectionItem<Person>(org, null));
        }

        [Test]
        public void VerifyHashCode()
        {
            Assert.AreEqual(target.GetHashCode(), new DifferenceCollectionItem<Person>(org, curr).GetHashCode());
            Assert.AreNotEqual(target.GetHashCode(), new DifferenceCollectionItem<Person>(null, curr).GetHashCode());
        }

        [Test]
        public void VerifyEqualsNull()
        {
            Assert.IsFalse(target.Equals(null));
        }

        [Test]
        public void ExoticEqualsScenariosWhenItemsAreNull()
        {
            Assert.IsFalse(
                new DifferenceCollectionItem<Person>(null, curr).Equals(
                    new DifferenceCollectionItem<Person>(org, curr)));
            Assert.IsFalse(
                new DifferenceCollectionItem<Person>(org, null).Equals(
                    new DifferenceCollectionItem<Person>(org, curr)));
            Assert.IsFalse(
                new DifferenceCollectionItem<Person>(org, curr).Equals(
                    new DifferenceCollectionItem<Person>(null, curr)));
            Assert.IsFalse(
                new DifferenceCollectionItem<Person>(org, curr).Equals(
                    new DifferenceCollectionItem<Person>(org, null)));

        }

        [Test]
        public void VerifyConstructorThrowsIfBothAreNull()
        {
            Assert.Throws<ArgumentException>(() => new DifferenceCollectionItem<Person>(null, null));
        }
    }
}