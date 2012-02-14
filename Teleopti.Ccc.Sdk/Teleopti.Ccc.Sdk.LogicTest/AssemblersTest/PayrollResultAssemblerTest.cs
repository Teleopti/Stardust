using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PayrollResultAssemblerTest
    {
        private PayrollResultAssembler target;
        private PayrollResult payrollResult;
        private IPayrollResultRepository repMock;
        private MockRepository mocks;
        private PayrollResultDto payrollResultDto;
        private Guid guid;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            repMock = mocks.StrictMock<IPayrollResultRepository>();
            target = new PayrollResultAssembler(repMock, new PayrollResultDetailAssembler());
            payrollResult = new PayrollResult(new PayrollExport(), new Person(), new DateTime(2010,12,27,12,23,1,2));
            payrollResult.FinishedOk = false;
            payrollResult.AddDetail(new PayrollResultDetail(DetailLevel.Error,"Hell", new DateTime(2010,12,27,12,44,1,2), new Exception("Error")));
            guid = new Guid("35B32515-CA95-46E4-9BE4-4BA0501A6544");
            payrollResultDto = new PayrollResultDto();
            payrollResultDto.Id = guid;
            payrollResult.SetId(guid);
        }

        [Test]
        public void ShouldConvertToDto()
        {
            var dto = target.DomainEntityToDto(payrollResult);
            Assert.AreEqual(dto.Timestamp, payrollResult.Timestamp);
            Assert.AreEqual(dto.HasError, payrollResult.HasError());
            Assert.AreEqual(dto.FinishedOk, payrollResult.FinishedOk);
            Assert.AreEqual(dto.IsWorking, payrollResult.IsWorking());
            Assert.AreEqual(dto.Id, payrollResult.Id);
            //Assert.AreEqual(dto.Details.First().DetailLevel, payrollResult.Details.First().DetailLevel);
            //Assert.AreEqual(dto.Details.First().ExceptionMessage, payrollResult.Details.First().ExceptionMessage);
            //Assert.AreEqual(dto.Details.First().ExceptionStackTrace, payrollResult.Details.First().ExceptionStackTrace);
            //Assert.AreEqual(dto.Details.First().Message, payrollResult.Details.First().Message);
            //Assert.AreEqual(dto.Details.First().Timestamp, payrollResult.Details.First().Timestamp);
        }

        [Test]
        public void  ShouldConvertToDomain()
        {
            IPayrollResult domain;
            using (mocks.Record())
            {
                Expect.Call(repMock.Get(guid)).Return(payrollResult);
            }
            using (mocks.Playback())
            {
                domain = target.DtoToDomainEntity(payrollResultDto);
            }
            Assert.AreEqual(domain.Id, payrollResultDto.Id);
        }
    }
}
