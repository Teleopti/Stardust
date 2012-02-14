using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class MultiplicatorDtoTest
    {
        private MultiplicatorDto target;
        private IMultiplicator multiplicator;

        [SetUp]
        public void Setup()
        {
            multiplicator = MultiplicatorFactory.CreateMultiplicator("Overtime", "OT1", Color.Firebrick,
                                                                     MultiplicatorType.Overtime, 1.5d);
            multiplicator.ExportCode = "371";
            target = new MultiplicatorDto(multiplicator);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("Overtime",target.Name);
            Assert.AreEqual(1.5d,target.Multiplicator);
            Assert.AreEqual(MultiplicatorTypeDto.Overtime,target.MultiplicatorType);
            Assert.AreEqual("371",target.PayrollCode);
            Assert.IsTrue(Color.Firebrick.A.Equals(target.Color.ToColor().A));
        }
    }
}