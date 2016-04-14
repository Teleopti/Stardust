using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AbsenceDtoTest
    {
        [Test]
        public void VerifyProperties()
        {
			var target = new AbsenceDto();
			target.Name = "lång";
            Assert.AreEqual("lång", target.Name);

            target.ShortName = "kort";
            Assert.AreEqual("kort", target.ShortName);

            target.Priority = 88;
            Assert.AreEqual(88, target.Priority);

            target.PayrollCode = "aabbbcc";
            Assert.AreEqual("aabbbcc",target.PayrollCode );
            
            target.DisplayColor = new ColorDto(Color.DeepPink);
            Assert.AreEqual(Color.DeepPink.ToArgb(), target.DisplayColor.ToColor().ToArgb());

            target.InContractTime = false;
            Assert.AreEqual(false, target.InContractTime);

            target.InContractTime = true;
            Assert.AreEqual(true, target.InContractTime);

            target.InPaidTime = true;
            Assert.AreEqual(true, target.InPaidTime);

            target.InWorkTime = true;
            Assert.AreEqual(true, target.InWorkTime);
        }
    }
}