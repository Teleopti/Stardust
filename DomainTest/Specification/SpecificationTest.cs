using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.DomainTest.Specification
{
    /// <summary>
    /// Tests for Specification
    /// </summary>
    [TestFixture]
    public class SpecificationTest
    {
        private Specification<IPerson> falseSpec;
        private Specification<IPerson> trueSpec;
        private List<IPerson> list;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            falseSpec = new DummySpecification(false);
            trueSpec = new DummySpecification(true);
            list = new List<IPerson>();
            list.Add(PersonFactory.CreatePerson());
            list.Add(PersonFactory.CreatePerson());
        }

        /// <summary>
        /// Verifies the andspecification works.
        /// </summary>
        [Test]
        public void VerifyAndSpecificationWorks()
        {
            Assert.AreEqual(0, list.FindAll(trueSpec.And(falseSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(0, list.FindAll(falseSpec.And(falseSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(0, list.FindAll(falseSpec.And(trueSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(2, list.FindAll(trueSpec.And(trueSpec).IsSatisfiedBy).Count);
        }

        [Test]
        public void VerifyAndSpecificationWorksUsingExtension()
        {
            IList<IPerson> casted = list;
            Assert.AreEqual(0, casted.FilterBySpecification(trueSpec.And(falseSpec)).Count());
            Assert.AreEqual(0, casted.FilterBySpecification(falseSpec.And(falseSpec)).Count());
            Assert.AreEqual(0, casted.FilterBySpecification(falseSpec.And(trueSpec)).Count());
            Assert.AreEqual(2, casted.FilterBySpecification(trueSpec.And(trueSpec)).Count());
        }

        /// <summary>
        /// Verifies the orspecification works.
        /// </summary>
        [Test]
        public void VerifyOrSpecificationWorks()
        {
            Assert.AreEqual(2, list.FindAll(trueSpec.Or(falseSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(0, list.FindAll(falseSpec.Or(falseSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(2, list.FindAll(falseSpec.Or(trueSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(2, list.FindAll(trueSpec.Or(trueSpec).IsSatisfiedBy).Count);
        }

        /// <summary>
        /// Verifies the and notspecification works.
        /// </summary>
        [Test]
        public void VerifyAndNotSpecificationWorks()
        {
            Assert.AreEqual(2, list.FindAll(trueSpec.AndNot(falseSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(0, list.FindAll(falseSpec.AndNot(falseSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(0, list.FindAll(falseSpec.AndNot(trueSpec).IsSatisfiedBy).Count);
            Assert.AreEqual(0, list.FindAll(trueSpec.AndNot(trueSpec).IsSatisfiedBy).Count);
        }

        private class DummySpecification : Specification<IPerson>
        {
            private readonly bool _isSatisfied;

            public DummySpecification(bool isSatisfied)
            {
                _isSatisfied = isSatisfied;
            }

            public override bool IsSatisfiedBy(IPerson obj)
            {
                return _isSatisfied;
            }
        }
    }
}