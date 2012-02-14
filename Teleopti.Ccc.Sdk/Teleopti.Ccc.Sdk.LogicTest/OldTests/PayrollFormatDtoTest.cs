using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class PayrollFormatDtoTest
    {
        private PayrollFormatDto target;
        private const string _name = "Teleopti Generic Payroll export";
        private readonly Guid _exportFormatId = Guid.NewGuid();
        
        [SetUp]
        public void Setup()
        {
            target = new PayrollFormatDto(_exportFormatId, _name);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_name,target.Name);
            Assert.AreEqual(_exportFormatId,target.FormatId);
        }
    }
}