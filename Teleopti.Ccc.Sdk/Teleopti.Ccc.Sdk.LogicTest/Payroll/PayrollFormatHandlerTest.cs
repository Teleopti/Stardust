using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Payroll;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.Payroll
{
    [TestFixture]
    public class PayrollFormatHandlerTest
    {
        [Test]
        public void ShouldReadPayrollFormatsFromStorage()
        {
			var formatHandler = new PayrollFormatHandler(new FakePayrollFormatRepository(), new FakeCurrentUnitOfWorkFactory());
			var payrollFormatDtos = new List<PayrollFormatDto> { new PayrollFormatDto(new Guid("1E88583F-284C-4C17-BD07-B7AFA08D0BF4"), "Sweet Päjrållformat", "tjo") };
            //First Save a file
            formatHandler.Save(payrollFormatDtos);

            var formats =  formatHandler.Load("tjo");
            Assert.AreEqual("Sweet Päjrållformat", formats.First().Name);
            Assert.AreEqual(new Guid("1E88583F-284C-4C17-BD07-B7AFA08D0BF4"), formats.First().FormatId);
        }
    }
}
