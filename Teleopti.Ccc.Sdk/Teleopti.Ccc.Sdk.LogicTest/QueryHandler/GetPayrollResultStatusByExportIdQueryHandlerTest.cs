using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPayrollResultStatusByExportIdQueryHandlerTest
	{
		private MockRepository mocks;
		private IPayrollExportRepository payrollExportRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetPayrollResultStatusByExportIdQueryHandler target;
		private IAssembler<IPayrollResult, PayrollResultDto> assembler;
		private IPayrollResultRepository payrollResultRepository;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			payrollExportRepository = mocks.DynamicMock<IPayrollExportRepository>();
			payrollResultRepository = mocks.DynamicMock<IPayrollResultRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			assembler = mocks.DynamicMock<IAssembler<IPayrollResult, PayrollResultDto>>();
			target = new GetPayrollResultStatusByExportIdQueryHandler(assembler, payrollResultRepository, payrollExportRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetAllPayrollExports()
		{
			var payrollExport = new PayrollExport();
			payrollExport.SetId(Guid.NewGuid());
			var payrollResult = new PayrollResult(payrollExport, null, DateTime.UtcNow);
			var payrollExportList = new List<IPayrollResult> {payrollResult};
			var dto = new PayrollResultDto();
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			using (mocks.Record())
			{
				Expect.Call(payrollExportRepository.Get(payrollExport.Id.GetValueOrDefault())).Return(payrollExport);
				Expect.Call(assembler.DomainEntitiesToDtos(payrollExportList)).Return(new[] {dto});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPayrollResultStatusByExportIdQueryDto{PayrollExportId = payrollExport.Id.GetValueOrDefault()});
				result.First().Should().Be.EqualTo(dto);
			}
		}
	}
}