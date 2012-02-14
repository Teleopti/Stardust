using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class ColorDtoTest
    {
        private ColorDto _target;
        private Color    _color;

        [SetUp]
        public void Setup()
        {
            _color = Color.DarkBlue;
            _target = new ColorDto(Color.DarkBlue);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_color.R,_target.Red);
            Assert.AreEqual(_color.G, _target.Green);
            Assert.AreEqual(_color.B, _target.Blue);
            Assert.AreEqual(_color.A, _target.Alpha);
        }   

        [Test]
        public void VerifyCanSetProperties()
        {
            _target.Red = Color.Yellow.R;
            Assert.AreEqual(Color.Yellow.R, _target.Red);
            _target.Green = Color.Yellow.G;
            Assert.AreEqual(Color.Yellow.G, _target.Green);
            _target.Blue = Color.Yellow.B;
            Assert.AreEqual(Color.Yellow.B, _target.Blue);
            _target.Alpha = Color.Yellow.A;
            Assert.AreEqual(Color.Yellow.A, _target.Alpha);
        }   
    }
}