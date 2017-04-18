using System;
using System.Windows;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.WinCodeTest.Converters.DateTimeConverter
{
    [TestFixture]
    public class DateTimeBaseConverterTest
    {

        private DateTimeBaseConverter _target;
        private TimeZoneInfo _testTimeZone;
        private TimeSpan _diff;
        private DateTime _utcDateTime;
       

        [SetUp]
        public void Setup()
        {
          
            _utcDateTime = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            _target = new TestConverter();
            _diff = TimeSpan.FromMinutes(33); //Timezone thats not the localtimezone for sure
            _testTimeZone = TimeZoneInfo.CreateCustomTimeZone("test", _diff, "test", "test");
        }

        [Test]
        public void VerifyTranslatesWithLocalTimeZoneByDefault()
        {
            Assert.AreEqual(TimeZoneInfo.Local, _target.ConverterTimeZone);
        }

        [Test]
        public void VerifyConvertsDateTime()
        {
            DateTime convertedDateTime = (DateTime)_target.Convert(_utcDateTime, typeof(DateTime), null, null);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeFromUtc(_utcDateTime, _target.ConverterTimeZone), convertedDateTime);
            _target.ConverterTimeZone = _testTimeZone;
            convertedDateTime = (DateTime)_target.Convert(_utcDateTime, typeof(DateTime), null, null);
            Assert.AreEqual(_diff.TotalMinutes, convertedDateTime.Subtract(_utcDateTime).TotalMinutes);
        }

        [Test]
        public void VerifyConvertBack()
        {
            _target.ConverterTimeZone = _testTimeZone;
            var convertedTime = _target.Convert(_utcDateTime, typeof(DateTime), null, null);
            DateTime convertedBack = (DateTime)_target.ConvertBack(convertedTime, typeof(DateTime), null, null);
            Assert.AreEqual(DateTimeKind.Utc, convertedBack.Kind);
            Assert.AreEqual(_utcDateTime, convertedBack);
        }

        [Test]
        public void VerifyConvertBackReturnsTheSameValueIfUtc()
        {
            var convertedTime = _target.ConvertBack(_utcDateTime, typeof(DateTime), _testTimeZone, null);
            Assert.AreEqual(_utcDateTime, convertedTime);
        }

        [Test]
        public void VerifyTransform()
        {
            string myPrameter = "parameter";

            TimeSpan hookTime = TimeSpan.FromMinutes(2);
            var result1 = _target.Convert(_utcDateTime, typeof(DateTime), null, null);
            ((TestConverter) _target).TestHook = hookTime;
            var result2 = _target.Convert(_utcDateTime, typeof(DateTime), myPrameter, null);
            Assert.AreEqual(((DateTime) result1).Add(hookTime), result2,
                            "HookTime should have been added on transform to result2");
           
            Assert.AreEqual(myPrameter, ((TestConverter)_target).Parameter,"Verify that the parameter is passed to the Transform");
            
        }

        [Test]
        public void VerifyTransformBack()
        {
            string myPrameter = "parameter";
            TimeSpan hookTime = TimeSpan.FromMinutes(2);
            var result1 = _target.ConvertBack(_utcDateTime, typeof(DateTime), null, null);
            ((TestConverter)_target).TestHook = hookTime;
            var result2 = _target.ConvertBack(_utcDateTime, typeof(DateTime), myPrameter, null);
            Assert.AreEqual(((DateTime)result1).Add(hookTime), result2,
                            "HookTime should have been added on transform to result2");

            Assert.AreEqual(myPrameter, ((TestConverter)_target).Parameter, "Verify that the parameter is passed to the TransformBack");

        }

        #region multibinding

        [Test]
        public void VerifyCanSetTimeZoneWithMultipleBinding()
        {
            object[] values = new object[] {_utcDateTime, _testTimeZone,"some idiot parameter"};
            _target.ConverterTimeZone = _testTimeZone;
            DateTime convertedDateTime = (DateTime)_target.Convert(_utcDateTime, typeof(DateTime), null, null);
            DateTime convertedDateTime2 = (DateTime) _target.Convert(values, typeof (DateTime), null, null);
            Assert.AreEqual(convertedDateTime, convertedDateTime2);
            
            values = new object[]{_utcDateTime,TimeZoneInfo.Utc};
            Assert.AreEqual(_utcDateTime,_target.Convert(values, typeof (DateTime), null, null));
        }

        [Test]
        public void VerifyUsesNormalBindingIfOnlyOneBinding()
        {
            object[] values = new object[] { _utcDateTime};
            _target.ConverterTimeZone = _testTimeZone;
            DateTime convertedDateTime = (DateTime)_target.Convert(_utcDateTime, typeof(DateTime), null, null);
            DateTime convertedDateTime2 = (DateTime)_target.Convert(values, typeof(DateTime), null, null);
            Assert.AreEqual(convertedDateTime, convertedDateTime2);
        }

        [Test]
        public void VerifyUsesNormalBindingIfTimeZoneInfoIsNull()
        {
            object[] values = new object[] { _utcDateTime,null };
            _target.ConverterTimeZone = _testTimeZone;
            DateTime convertedDateTime = (DateTime)_target.Convert(_utcDateTime, typeof(DateTime), null, null);
            DateTime convertedDateTime2 = (DateTime)_target.Convert(values, typeof(DateTime), null, null);
            Assert.AreEqual(convertedDateTime, convertedDateTime2);
        }
    

        [Test]
        public void VerifyConvertBackMultipleBinding()
        {
            object[] values = new object[] { _utcDateTime, _testTimeZone, "some idiot binding" };
            var dateTime =  _target.Convert(values, typeof(DateTime), null, null);
            object[] ret = _target.ConvertBack(dateTime, new Type[] {typeof(DateTime),typeof(TimeZoneInfo)}, null, null);
            Assert.AreEqual(_utcDateTime,ret[0]);
            Assert.AreEqual(_testTimeZone, ret[1]);

        }

        #endregion //multibinding

        #region parameters
        [Test]
        public void VerifyValueMustBeDateTime()
        {
	        Assert.Throws<ArgumentException>(() =>
	        {
				var dateTime = _target.Convert("not a dateTime", typeof(DateTime), null, null);
				Assert.AreEqual(dateTime, _target, "FxCop");
	        });
        }

        [Test]
        public void VerifyParametersForMultipleBinding()
        {
	        Assert.Throws<ArgumentException>(() =>
	        {
				object[] values = new object[] {_utcDateTime, "not a timezone"};
				_target.ConverterTimeZone = _testTimeZone;
				DateTime convertedDateTime = (DateTime) _target.Convert(values, typeof(DateTime), null, null);
				Assert.AreEqual(convertedDateTime, _target, "FxCop");
	        });
        }

        #endregion

    }

    public class TestConverter : DateTimeBaseConverter
    {

        public TimeSpan TestHook
        {
            get { return (TimeSpan)GetValue(TestHookProperty); }
            set { SetValue(TestHookProperty, value); }
        }

        public object Parameter { get; private set; }

        public static readonly DependencyProperty TestHookProperty =
            DependencyProperty.Register("TestHook", typeof(TimeSpan), typeof(TestConverter), new UIPropertyMetadata(TimeSpan.Zero));

        public DateTime ConvertedDateTime { get; set; }

        public override object Transform(DateTime convertedDateTime,object parameter)
        {
            Parameter = parameter;
            return convertedDateTime.Add(TestHook);
        }

        public override object TransformBack(DateTime convertedDateTime,object parameter)
        {
            Parameter = parameter;
            return convertedDateTime.Add(TestHook);
        }
        
    }

  
}
