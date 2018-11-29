using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;


namespace Teleopti.Ccc.DomainTest.Scheduling.TimeLayer
{
    [TestFixture]
    public class MultiplicatorLayerTest
    {
        private MultiplicatorLayer _target;
        private IMultiplicator _multiplicator;
        private DateTimePeriod _dtp;
        private IMultiplicatorDefinitionSet _definitionSet;

        [SetUp]
        public void Setup()
        {
            _multiplicator = new Multiplicator(MultiplicatorType.OBTime);
            _multiplicator.Description = new Description("Test");
            _multiplicator.DisplayColor = Color.Blue;
            _multiplicator.ExportCode = "Code";
            _multiplicator.MultiplicatorValue = 3.5;
            _definitionSet = new MultiplicatorDefinitionSet("test", _multiplicator.MultiplicatorType);
            _dtp = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
        }

        [Test]
        public void VerifyConstructor()
        {
            _target = new MultiplicatorLayer(_definitionSet, _multiplicator, _dtp);
            Assert.AreEqual(3.5d, _target.Payload.MultiplicatorValue);
            Assert.AreEqual(_dtp, _target.Period);
            Assert.AreEqual(_dtp,_target.LayerOriginalPeriod);
        }

        [Test]
        public void VerifyCanSetLayerOriginalPeriod()
        {
            _target = new MultiplicatorLayer(_definitionSet, _multiplicator, _dtp);

            DateTimePeriod newDtp = new DateTimePeriod(2000,1,3,2000,1,4);
            _target.LayerOriginalPeriod = newDtp;
            Assert.AreEqual(newDtp , _target.LayerOriginalPeriod);
        }
    }
}
