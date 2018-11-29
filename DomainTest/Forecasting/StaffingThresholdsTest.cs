using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class StaffingThresholdsTest
    {
        private StaffingThresholds _target;
        private Percent _seriousUnderstaffing;
        private Percent _understaffing;
        private Percent _overstaffing;
    	private Percent _understaffingFor;

    	[SetUp]
        public void Setup()
        {
            _seriousUnderstaffing = new Percent(0.2);
            _understaffing = new Percent(0.3);
            _overstaffing = new Percent(0.4);
        	_understaffingFor = new Percent(0.75);
            _target = new StaffingThresholds(_seriousUnderstaffing,_understaffing,_overstaffing);
        }

        [Test]
        public void VerifyProperties()
        {
            _target = new StaffingThresholds();

            Assert.AreEqual(new Percent(),_target.SeriousUnderstaffing);
            Assert.AreEqual(new Percent(), _target.Understaffing);
            Assert.AreEqual(new Percent(), _target.Overstaffing);

            _target = new StaffingThresholds(_seriousUnderstaffing,_understaffing,_overstaffing);

            Assert.AreEqual(_target.SeriousUnderstaffing,_seriousUnderstaffing);
            Assert.AreEqual(_target.Understaffing,_understaffing);
            Assert.AreEqual(_target.Overstaffing, _overstaffing);

			_target = new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing, _understaffingFor);
			Assert.AreEqual(_target.SeriousUnderstaffing, _seriousUnderstaffing);
			Assert.AreEqual(_target.Understaffing, _understaffing);
			Assert.AreEqual(_target.Overstaffing, _overstaffing);
			Assert.AreEqual(_target.UnderstaffingFor, _understaffingFor);
        }

        [Test]
        public void VerifyNegativeValuesAreNotInterpretedAsAbsolute()
        {
            var target = new StaffingThresholds(new Percent(-_seriousUnderstaffing.Value),
                                            new Percent(-_understaffing.Value),
                                            new Percent(-_overstaffing.Value));

			Assert.AreNotEqual(_seriousUnderstaffing, target.SeriousUnderstaffing);
			Assert.AreNotEqual(_understaffing, target.Understaffing);
			Assert.AreNotEqual(_overstaffing, target.Overstaffing);
        }

        [Test]
        public void VerifyDefaultValues()
        {
            _target = StaffingThresholds.DefaultValues();
            Assert.AreEqual(new Percent(-0.2), _target.SeriousUnderstaffing);
            Assert.AreEqual(new Percent(-0.1), _target.Understaffing);
            Assert.AreEqual(new Percent(0.1), _target.Overstaffing);
            Assert.AreEqual(new Percent(1.0), _target.UnderstaffingFor);
        }

        [Test]
        public void VerifyEquals()
        {
            var target2 = new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing);
            Assert.IsTrue(_target.Equals(target2));
            Assert.IsFalse(_target.Equals(null));
            Assert.IsFalse(_target.Equals(new StaffingThresholds(_seriousUnderstaffing,_understaffing,new Percent())));
            Assert.IsFalse(_target.Equals((object)new StaffingThresholds(_seriousUnderstaffing, new Percent(0.1), _overstaffing)));
            Assert.IsFalse(new StaffingThresholds(new Percent(0.02), new Percent(0.03), new Percent(0.04)).Equals(_target));
			

            Assert.IsFalse(_target.Equals(null));
            Assert.IsFalse(_target.Equals(new StaffingThresholds(_seriousUnderstaffing,_understaffing,new Percent(), _understaffingFor)));
            Assert.IsFalse(_target.Equals((object)new StaffingThresholds(_seriousUnderstaffing, new Percent(0.1), _overstaffing, _understaffingFor)));
            Assert.IsFalse(new StaffingThresholds(new Percent(0.02), new Percent(0.03), new Percent(0.04), new Percent(0.75)).Equals(_target));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            var thresholds = new StaffingThresholds(_seriousUnderstaffing, new Percent(0.1), _overstaffing);
            IDictionary<StaffingThresholds, int> dic = new Dictionary<StaffingThresholds, int>();
            dic[thresholds] = 5;
            dic[_target] = 8;
            Assert.AreEqual(5, dic[thresholds]);
        }

        [Test]
        public void VerifyOperators()
        {
            Assert.IsFalse(new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing) == new StaffingThresholds(_seriousUnderstaffing, new Percent(0.15), _overstaffing));
            Assert.IsTrue(new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing) != new StaffingThresholds(_seriousUnderstaffing, new Percent(0.15), _overstaffing));
			Assert.IsFalse(new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing, _understaffingFor) == new StaffingThresholds(_seriousUnderstaffing, new Percent(0.15), _overstaffing, _understaffingFor));
			Assert.IsTrue(new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing, _understaffingFor) != new StaffingThresholds(_seriousUnderstaffing, new Percent(0.15), _overstaffing, _understaffingFor));
			Assert.IsTrue(new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing) != new StaffingThresholds(_seriousUnderstaffing, _understaffing, _overstaffing, _understaffingFor));
		}
    }
}
