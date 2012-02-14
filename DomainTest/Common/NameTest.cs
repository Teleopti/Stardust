using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for Name
    /// </summary>
    [TestFixture]
    public class NameTest
    {
        private Name target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            //init
            target = new Name();
        }

        /// <summary>
        /// Verifies the first and last name properties.
        /// </summary>
        [Test]
        public void VerifyFirstAndLastNameProperties()
        {
            string first = "Roger";
            string last = "KRatz";
            target = new Name(first, last);
            Assert.AreEqual(first, target.FirstName);
            Assert.AreEqual(last, target.LastName);
        }

        /// <summary>
        /// Verifies to string.
        /// </summary>
        [Test]
        public void VerifyToString()
        {
            string first = "Roger";
            string last = "KRatz";
            target = new Name(first, last);

            Assert.AreEqual(first + " " + last, target.ToString());
        }

        /// <summary>
        /// Verifies to string.
        /// </summary>
        [Test]
        public void VerifyToStringWithLastNameFirst()
        {
            string first = "Roger";
            string last = "Kratz";
            target = new Name(first, last);

            Assert.AreEqual(last + ", " + first, target.ToString(NameOrderOption.LastNameFirstName));
        }

        /// <summary>
        /// Verifies ToString() when firstname doesn't exist.
        /// </summary>
        [Test]
        public void VerifyToStringWithNoFirstName()
        {
            target = new Name(null, "Kratz");
            Assert.AreEqual("Kratz", target.ToString());
        }

        /// <summary>
        /// Verifies ToString() when lastname doesn't exist.
        /// </summary>
        [Test]
        public void VerifyToStringWithNoLastName()
        {
            target = new Name("Roger", null);
            Assert.AreEqual("Roger", target.ToString());
        }

        /// <summary>
        /// Verifies the struct's initialized value.
        /// </summary>
        [Test]
        public void VerifyInitializedValue()
        {
            Assert.AreEqual(string.Empty, target.FirstName);
            Assert.AreEqual(string.Empty, target.LastName);
            Assert.AreEqual(string.Empty, target.ToString());
        }

        /// <summary>
        /// Verifies the equals method.
        /// </summary>
        [Test]
        public void VerifyEquals()
        {
            target = new Name("roger", "kratz");
            Name target2 = new Name("roger", "kratz");
            Assert.IsTrue(target.Equals(target2));
            Assert.IsFalse(target.Equals(null));
            Assert.IsFalse(target.Equals(new Name(null, "Kratz")));
            Assert.IsFalse(target.Equals(new Name("roger", string.Empty)));
            Assert.IsFalse(target.Equals(new Name("roger", "lff")));
            Assert.IsFalse(new Name("ff", "ff").Equals(target));
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            Name name = new Name("sr", "ff");
            Name name2 = new Name("sr", "ff");
            IDictionary<Name, int> dic = new Dictionary<Name, int>();
            dic[name] = 5;
            dic[name2] = 8;
            Assert.AreEqual(8, dic[name]);
        }

        /// <summary>
        /// Verifies the operators.
        /// </summary>
        [Test]
        public void VerifyOperators()
        {
            Assert.IsTrue(new Name() == new Name());
            Assert.IsFalse(new Name("ff", "") == new Name("ff", "ff"));
            Assert.IsTrue(new Name(null, "kratz") != new Name("kratz", null));
        }
    }
}