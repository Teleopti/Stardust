using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AbsenceDtoTest
    {
        private AbsenceDto  _target;

        [SetUp]
        public void Setup()
        {
            _target = new AbsenceDto();
        }

        [Test]
        public void VerifyProperties()
        {
            _target.Name = "lång";
            Assert.AreEqual("lång", _target.Name);

            _target.ShortName = "kort";
            Assert.AreEqual("kort", _target.ShortName);

            _target.Priority = 88;
            Assert.AreEqual(88, _target.Priority);

            _target.PayrollCode = "aabbbcc";
            Assert.AreEqual("aabbbcc",_target.PayrollCode );
            
            _target.DisplayColor = new ColorDto(Color.DeepPink);
            Assert.AreEqual(Color.DeepPink.ToArgb(), _target.DisplayColor.ToColor().ToArgb());

            _target.InContractTime = false;
            Assert.AreEqual(false, _target.InContractTime);

            _target.InContractTime = true;
            Assert.AreEqual(true, _target.InContractTime);

            _target.InPaidTime = true;
            Assert.AreEqual(true, _target.InPaidTime);

            _target.InWorkTime = true;
            Assert.AreEqual(true, _target.InWorkTime);
        }
    }
}