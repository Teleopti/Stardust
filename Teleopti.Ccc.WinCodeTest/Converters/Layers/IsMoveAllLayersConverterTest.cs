using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.Layers;


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
			Assert.AreEqual(1, (double)_target.Convert(false, typeof(double), d, null));
			Assert.AreEqual(d, (double)_target.Convert(true, typeof(double), d, null));
		}

		[Test]
		public void VerifyWorksWithInt()
		{
			int i = 5;
			Assert.AreEqual(1, (double)_target.Convert(false, typeof(double), i, null));
			Assert.AreEqual(i, (double)_target.Convert(true, typeof(double), i, null));
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
		public void ParameterMustBeDouble()
		{
			Assert.Throws<FormatException>(() =>
			{
				Assert.AreEqual(1, (double)_target.Convert(true, typeof(double), "not double", null));
			});
		}

		[Test]
		public void ValueMustBeBool()
		{
			Assert.Throws<FormatException>(() =>
			{
				Assert.AreEqual(1, (double)_target.Convert("not bool", typeof(double), 1, null));
			});
		}

		[Test]
		public void ConvertBackIsNotImplemented()
		{
			Assert.Throws<NotImplementedException>(() =>
			{
				Assert.AreEqual(1, (double)_target.ConvertBack(false, typeof(double), 5d, null));
			});
		}
	}
}
