using NUnit.Framework;
using System.Globalization;
using System.Threading;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture,SetCulture("sv-SE")]
    public class PercentTest
    {
        private Percent target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new Percent(0.764);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(0.764, target.Value);
        }

        [Test]
        public void VerifyToString()
        {
            Assert.AreEqual("76 %", target.ToString());
        }

	    [Test]
	    public void ShouldReturnValueAsPercent()
	    {
		    target.ValueAsPercent()
			    .Should().Be.EqualTo(76.4);
	    }

        [Test]
        public void VerifyEqualsWorks()
        {
            Percent target2;
            target2 = new Percent(0.764);
            Assert.IsTrue(target == target2);
            Assert.IsFalse(target!= target2);
            Assert.IsTrue(target.Equals(target2));
            Assert.IsTrue(target.Equals((object)target2));
            Assert.IsFalse(target.Equals(4));
            Assert.IsFalse(target.Equals(null));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            Percent target2 = new Percent(0.764);
            Assert.AreEqual(target.GetHashCode(), target2.GetHashCode());
        }

        [Test]
        public void VerifyCompareTo()
        {
            double testValue = 3;
            Percent target2 = new Percent(testValue);
            Assert.AreEqual(testValue.CompareTo(0.764), target2.CompareTo(target));
            Assert.IsTrue(target2>target);
            Assert.IsFalse(target2<target);
        }

        /// <summary>
        /// Verifies the parse works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        [Test]
        public void VerifyParseWorks()
        {
            string testValue = "82{0}5{1}";
            double expectedResult = 0.825d;

            CultureInfo threadCulture = CultureInfo.CurrentCulture;
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                Thread.CurrentThread.CurrentCulture = ci;
                Percent result;
                Assert.IsTrue(Percent.TryParse(string.Format(ci, testValue,ci.NumberFormat.PercentDecimalSeparator, ci.NumberFormat.PercentSymbol), out result));
                Assert.AreEqual(expectedResult, result.Value);
            }
            Thread.CurrentThread.CurrentCulture = threadCulture;
        }


		[Test]
		public void ShouldParseValueWhenDoubleValueWithComma()
		{
			string testValue = "82,53";

			Percent result;
			Assert.IsTrue(Percent.TryParse(testValue, out result, true));
			Assert.AreEqual(0.8253, result.Value);
		}


		/// <summary>
		/// Verifies to string with decimals work.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-28
		/// </remarks>
		[Test]
        public void VerifyToStringWithDecimalsWork()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.PercentDecimalSeparator = ",";
            nfi.PercentGroupSeparator = " ";
            nfi.PercentDecimalDigits = 1;
            Assert.AreEqual("76,4 %", target.ToString(nfi));

            nfi.PercentDecimalDigits = 2;
            Assert.AreEqual("76,40 %", target.ToString(nfi));
        }

        [Test]
        public void VerifyParseCanFail()
        {
            Percent result;
            Assert.IsFalse(Percent.TryParse("y", out result));
            Assert.AreEqual(new Percent(), result);
        }
    }
}
