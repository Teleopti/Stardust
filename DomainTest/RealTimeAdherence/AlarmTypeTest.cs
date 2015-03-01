using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class AlarmTypeTest
    {
        private AlarmType target;
        private readonly Description _description = new Description("Wrong state");
        private readonly Color _color = Color.DeepPink;
        private readonly TimeSpan _thresholdTime = TimeSpan.FromSeconds(150);
        private const double _staffingEffect= 1.0;

        [SetUp]
        public void Setup()
        {
            target = new AlarmType(_description,_color, _thresholdTime, _staffingEffect );
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_description,target.Description);
            Assert.AreEqual(_description,target.ConfidentialDescription(null,new DateOnly()));
            Assert.AreEqual(_color,target.DisplayColor);
            Assert.AreEqual(_color,target.ConfidentialDisplayColor(null,new DateOnly()));
            Assert.AreEqual(_thresholdTime, target.ThresholdTime);
            Assert.AreEqual(_staffingEffect ,target.StaffingEffect);
            Assert.IsNull(target.Tracker);
            Assert.IsFalse(target.InContractTime);

            Description description = new Description("My new description");
            Color color = Color.Firebrick;
            TimeSpan thresholdTime = TimeSpan.FromSeconds(73);
            AlarmTypeMode mode = AlarmTypeMode.Unknown;

            target.Description = description;
            target.DisplayColor = color;
            target.ThresholdTime = thresholdTime;
            target.Tracker = null;
            target.InContractTime = true;
            target.StaffingEffect = 0.8;


            Assert.AreEqual(description,target.Description);
            Assert.AreEqual(color,target.DisplayColor);
            Assert.AreEqual(thresholdTime,target.ThresholdTime);
            Assert.IsNull(target.Tracker);
            Assert.IsTrue(target.InContractTime);
            Assert.AreEqual(0.8,target.StaffingEffect );
        }

        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(),true));
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCannotHaveNegativeThresholdTime()
        {
            target.ThresholdTime = TimeSpan.FromSeconds(-20);
        }


        [Test]
        public void VerifySetDeleted()
        {
            target.SetDeleted();
            Assert.IsTrue(target.IsDeleted);
        }
    }
}
