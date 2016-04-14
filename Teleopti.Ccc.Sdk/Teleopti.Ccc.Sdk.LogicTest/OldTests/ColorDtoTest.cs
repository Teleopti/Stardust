using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class ColorDtoTest
    {
        [Test]
        public void VerifyConstructor()
        {
			var color = Color.DarkBlue;
			var target = new ColorDto(Color.DarkBlue);

			Assert.AreEqual(color.R,target.Red);
            Assert.AreEqual(color.G, target.Green);
            Assert.AreEqual(color.B, target.Blue);
            Assert.AreEqual(color.A, target.Alpha);
        }   

        [Test]
        public void VerifyCanSetProperties()
        {
			var target = new ColorDto(Color.DarkBlue);

			target.Red = Color.Yellow.R;
            Assert.AreEqual(Color.Yellow.R, target.Red);
            target.Green = Color.Yellow.G;
            Assert.AreEqual(Color.Yellow.G, target.Green);
            target.Blue = Color.Yellow.B;
            Assert.AreEqual(Color.Yellow.B, target.Blue);
            target.Alpha = Color.Yellow.A;
            Assert.AreEqual(Color.Yellow.A, target.Alpha);
        }   
    }
}