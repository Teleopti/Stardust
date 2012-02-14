using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class MathHelperTest
    {
        private IList<double> items;

        [Test]
        public void VerifyGetMinMax()
        {
            items = new List<double>{ -5, -100, +6, +2 };
            MinMax<double> result = MathHelper.GetMinMax(items).Value;
            Assert.AreEqual(-100, result.Minimum);
            Assert.AreEqual(+6, result.Maximum);
        }

        [Test]
        public void VerifyGetMinMaxWithNegativeValues()
        {
            items = new List<double> { -5, -100, -6, -2 };
            MinMax<double> result = MathHelper.GetMinMax(items).Value;
            Assert.AreEqual(-100, result.Minimum);
            Assert.AreEqual(-2, result.Maximum);
        }

        [Test]
        public void VerifyGetMinMaxWithPositiveValues()
        {
            items = new List<double> { +5, +100, +6, +2 };
            MinMax<double> result = MathHelper.GetMinMax(items).Value;
            Assert.AreEqual(+2, result.Minimum);
            Assert.AreEqual(+100, result.Maximum);
        }

        [Test]
        public void VerifyGetMinMaxWithNaNValues()
        {
            items = new List<double> { +5, +100, +6, +2, double.NaN };
            MinMax<double> result = MathHelper.GetMinMax(items).Value;
            Assert.AreEqual(+2, result.Minimum);
            Assert.AreEqual(+100, result.Maximum);

            items = new List<double> { double.NaN, double.NaN };
            Assert.IsNull(MathHelper.GetMinMax(items));

            items = new List<double> { double.PositiveInfinity, 12 };
            result = MathHelper.GetMinMax(items).Value;
            Assert.IsTrue(double.IsPositiveInfinity(result.Maximum));

            items = new List<double> { double.NegativeInfinity, 12 };
            result = MathHelper.GetMinMax(items).Value;
            Assert.IsTrue(double.IsNegativeInfinity(result.Minimum));
        }
         
    }
}
