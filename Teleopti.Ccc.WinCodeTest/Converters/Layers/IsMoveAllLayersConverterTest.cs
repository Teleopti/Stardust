using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Teleopti.Ccc.WinCode.Converters.Layers;


namespace Teleopti.Ccc.WinCodeTest.Converters.Layers
{
    [TestFixture]
    public class IsMoveAllLayersConverterTest
    {
        private IsMoveAllLayerConverter _target;

        [SetUp]
        public void Setup()
        {
            _target = new IsMoveAllLayerConverter();
        }
	
        [Test]
        public void VerifyConvertsToDoubleItsSizeWhenParameterIsSupplied()
        {
            double d = 5; 
            Assert.AreEqual(1,(double)_target.Convert(false, typeof(double),d,null));
            Assert.AreEqual(d,(double)_target.Convert(true, typeof(double),d,null));
        }

        [Test]
        public void VerifyWorksWithInt()
        {
            int i = 5;
            Assert.AreEqual(1, (double) _target.Convert(false, typeof (double), i, null));
            Assert.AreEqual(i, (double) _target.Convert(true, typeof (double), i, null));
        }

        [Test]
        public void VerifyCanChangeTheDefaultValue()
        {
            double doubleIfFalse = 12d;
            _target.ValueIfFalse = doubleIfFalse;
            Assert.AreEqual(doubleIfFalse, (double)_target.Convert(false, typeof(double), 15d, null));
        }

        //Exceptions
        [Test]
        [ExpectedException(typeof(FormatException))]
        public void ParameterMustBeDouble()
        {
            Assert.AreEqual(1, (double)_target.Convert(true, typeof(double), "not double", null));
        }
        [Test]
        [ExpectedException(typeof(FormatException))]
        public void ValueMustBeBool()
        {
            Assert.AreEqual(1, (double)_target.Convert("not bool", typeof(double), 1, null));
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConvertBackIsNotImplemented()
        {
            Assert.AreEqual(1, (double)_target.ConvertBack(false, typeof(double), 5d, null));
        }
    }
}
