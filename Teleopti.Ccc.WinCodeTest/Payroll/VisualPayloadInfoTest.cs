using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;


namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class VisualPayloadInfoTest
    {
        private VisualPayloadInfo _target;
        private DateTime _startDate;
        private DateTime _endDate;
        private Color _color;
        private string _value;
        private string _name;
        private string _shortName;
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _startDate = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _endDate = new DateTime(2008, 1, 31, 0, 0, 0, DateTimeKind.Utc);
            _color = Color.DarkViolet;
            _name = "name";
            _shortName = "shortName";
            _value = "value";
            _period = new DateTimePeriod();
            _target = new VisualPayloadInfo(_startDate,_endDate,_color,_name, _shortName,_value,_period);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            _target = new VisualPayloadInfo();
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            Rectangle rectangle = new Rectangle(10,10,10,10);
            _target.Bounds = rectangle;
            Assert.AreEqual(_value,_target.Value);
            Assert.AreEqual(_startDate,_target.StartTime);
            Assert.AreEqual(_endDate,_target.EndTime);
            Assert.AreEqual(_name,_target.Name);
            Assert.AreEqual(_shortName,_target.ShortName);
            Assert.AreEqual(_color,_target.Color);
            Assert.AreEqual(_period,_target.OriginalDateTimePeriod);
            Assert.AreEqual(rectangle, _target.Bounds);
        }
    }
}
