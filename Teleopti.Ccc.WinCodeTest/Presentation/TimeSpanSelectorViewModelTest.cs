using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class TimeSpanSelectorViewModelTest
    {

        private TimeSpanSelectorViewModel _target;
       

        [SetUp]
        public void Setup()
        {
           
            _target = new TimeSpanSelectorViewModel();
        }

        [Test]
        public void VerifyDefaultValues()
        {
            Assert.AreEqual(TimeSpan.FromMinutes(15), _target.TimeSpan);
        }

        [Test]
        public void VerifyCanSetGetTimeSpan()
        {
            TimeSpan newValue = TimeSpan.FromMinutes(7);
            Assert.IsFalse(newValue == _target.TimeSpan, "Verify defaultvalue is different from new value");
            _target.TimeSpan = newValue;
            Assert.AreEqual(newValue, _target.TimeSpan, "Verify new value is set");
        }

        [Test]
        public void VerifyMinMaxDefaultValues()
        {
            Assert.AreEqual(TimeSpan.FromMinutes(1), _target.MinMax.Minimum, "Minimum default 1 min");
            Assert.AreEqual(TimeSpan.FromMinutes(60), _target.MinMax.Maximum, "Maximum default 60 min");
        }

        [Test]
        public void VerifyCanSetGetMinMax()
        {
            MinMax<TimeSpan> newValue = new MinMax<TimeSpan>(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20));
            _target.MinMax = newValue;
            Assert.AreEqual(newValue, _target.MinMax);
        }

        [Test]
        public void VerifyCoerceTimeSpanWhenSettingMinMax()
        {
            _target.MinMax = new MinMax<TimeSpan>(TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(30));
            Assert.AreEqual(_target.MinMax.Minimum, _target.TimeSpan, "MinMax min set to higher than TimeSpan coerces to minimum");
            _target.MinMax = new MinMax<TimeSpan>(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(5));
            Assert.AreEqual(_target.MinMax.Maximum, _target.TimeSpan, "MinMax max set to lower than TimeSpan coerces to maximum");
        }

        [Test]
        public void VerifyCoerceValueWhenSettingTimeSpan()
        {
            _target.MinMax = new MinMax<TimeSpan>(TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(30));
            _target.TimeSpan = TimeSpan.FromMinutes(5);
            Assert.AreEqual(TimeSpan.FromMinutes(20), _target.TimeSpan, "Coerced to min when set lower than min");
            _target.TimeSpan = TimeSpan.FromMinutes(40);
            Assert.AreEqual(TimeSpan.FromMinutes(30), _target.TimeSpan, "Coerced to max when set higher than min");
        }

        [Test]
        public void VerifySnappingOnOff()
        {
            Assert.IsTrue(_target.Snap,"Default Snap==true");
            _target.Snap = false;
            Assert.IsFalse(_target.Snap);
        }

        

    }
}
