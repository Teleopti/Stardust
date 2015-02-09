using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for Description
    /// </summary>
    [TestFixture]
    public class DescriptionTest
    {
        private Description target;
        private string _name = "Semester";
        private string _shortName = "SEM";

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            //init
            target = new Description();
        }

        /// <summary>
        /// Verifies the name and short name properties.
        /// </summary>
        [Test]
        public void VerifyNameAndShortNameProperties()
        {
            target = new Description(_name, _shortName);
            Assert.AreEqual(_name, target.Name);
            Assert.AreEqual(_shortName, target.ShortName);
        }

        /// <summary>
        /// Verifies to string.
        /// </summary>
        [Test]
        public void VerifyToString()
        {
            target = new Description(_name, _shortName);

            Assert.AreEqual(_shortName + ", " + _name, target.ToString());
        }

        /// <summary>
        /// Verifies ToString() when short name doesn't exist.
        /// </summary>
        [Test]
        public void VerifyToStringWithNoShortName()
        {
            target = new Description(_name, null);
            Assert.AreEqual(_name, target.ToString());
        }

        /// <summary>
        /// Verifies the constant length getter works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-12
        /// </remarks>
        [Test]
        public void VerifyConstantLengthGetterWorks()
        {
            Assert.AreEqual(25, Description.MaxLengthOfShortName);
            Assert.AreEqual(50, Description.MaxLengthOfName);
        }

        /// <summary>
        /// Verifies the struct's initialized value.
        /// </summary>
        [Test]
        public void VerifyInitializedValue()
        {
            Assert.AreEqual(string.Empty, target.Name);
            Assert.AreEqual(string.Empty, target.ShortName);
            Assert.AreEqual(string.Empty, target.ToString());
        }

        /// <summary>
        /// Verifies the equals method.
        /// </summary>
        [Test]
        public void VerifyEquals()
        {
            target = new Description(_name, _shortName);
            Description target2 = new Description(_name, _shortName);
            Assert.IsTrue(target.Equals(target2));
            Assert.IsFalse(target.Equals(null));
            Assert.IsFalse(target.Equals(new Description(_name, string.Empty)));
            Assert.IsFalse(target.Equals((object)new Description(_name, "SEM2")));
            Assert.IsFalse(new Description("Vacation", "VAC1").Equals(target));
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            Description desc = new Description("sr", "ff");
            IDictionary<Description, int> dic = new Dictionary<Description, int>();
            dic[desc] = 5;
            dic[target] = 8;
            Assert.AreEqual(5, dic[desc]);
        }

        /// <summary>
        /// Verifies the operators.
        /// </summary>
        [Test]
        public void VerifyOperators()
        {
            Assert.IsTrue(new Description() == new Description());
            Assert.IsFalse(new Description("ff", "") == new Description("ff", "ff"));
            Assert.IsTrue(new Description(_name,null) != new Description(_shortName, null));
        }

        /// <summary>
        /// Verifies that too long name gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyTooLongNameGivesException()
        {
					target = new Description(String.Concat(Enumerable.Repeat("a", 55)), null);
        }

        /// <summary>
        /// Verifies that too long short name gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyTooLongShortNameGivesException()
        {
            target = new Description(_name, "".PadLeft(26));
        }

				[Test]
				[ExpectedException(typeof(ArgumentException))]
				public void ShouldNotAcceptOnlyWhitespacesInName()
				{
					target = new Description(" ");
				}
    }
}