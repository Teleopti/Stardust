using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PayrollResultAssemblerTest
    {
        [Test]
        public void ShouldConvertToDto()
        {
			var repMock = new FakePayrollResultRepository();
			var target = new PayrollResultAssembler(repMock, new PayrollResultDetailAssembler());
			var payrollResult = new PayrollResult(new PayrollExport(), new Person(), new DateTime(2010, 12, 27, 12, 23, 1, 2)).WithId();
			payrollResult.FinishedOk = false;
			payrollResult.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell", new DateTime(2010, 12, 27, 12, 44, 1, 2), new Exception("Error")));
			repMock.Add(payrollResult);

			var dto = target.DomainEntityToDto(payrollResult);
            Assert.AreEqual(dto.Timestamp, payrollResult.Timestamp);
            Assert.AreEqual(dto.HasError, payrollResult.HasError());
            Assert.AreEqual(dto.FinishedOk, payrollResult.FinishedOk);
            Assert.AreEqual(dto.IsWorking, payrollResult.IsWorking());
            Assert.AreEqual(dto.Id, payrollResult.Id);
        }

        [Test]
        public void  ShouldConvertToDomain()
		{
			var repMock = new FakePayrollResultRepository();
			var target = new PayrollResultAssembler(repMock, new PayrollResultDetailAssembler());
			var payrollResult = new PayrollResult(new PayrollExport(), new Person(), new DateTime(2010, 12, 27, 12, 23, 1, 2)).WithId();
			payrollResult.FinishedOk = false;
			payrollResult.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell", new DateTime(2010, 12, 27, 12, 44, 1, 2), new Exception("Error")));
			repMock.Add(payrollResult);

			var payrollResultDto = new PayrollResultDto();
			payrollResultDto.Id = payrollResult.Id.GetValueOrDefault();

			var domain = target.DtoToDomainEntity(payrollResultDto);

			Assert.AreEqual(domain.Id, payrollResultDto.Id);
        }
    }
}
