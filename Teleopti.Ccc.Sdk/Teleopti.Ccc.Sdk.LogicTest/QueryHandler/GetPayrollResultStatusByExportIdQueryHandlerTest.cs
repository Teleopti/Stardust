using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

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
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			payrollExportRepository = mocks.StrictMock<IPayrollExportRepository>();
			payrollResultRepository = mocks.StrictMock<IPayrollResultRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			assembler = mocks.DynamicMock<IAssembler<IPayrollResult, PayrollResultDto>>();
			target = new GetPayrollResultStatusByExportIdQueryHandler(assembler, payrollResultRepository, payrollExportRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetPayrollResultByPayrollExport()
		{
			var payrollExport = new PayrollExport();
			payrollExport.SetId(Guid.NewGuid());
			var payrollResult = new PayrollResult(payrollExport, null, DateTime.UtcNow);
			var payrollResultList = new List<IPayrollResult> {payrollResult};
			var dto = new PayrollResultDto();
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			using (mocks.Record())
			{
				Expect.Call(payrollExportRepository.Load(payrollExport.Id.GetValueOrDefault())).Return(payrollExport);
				Expect.Call(payrollResultRepository.GetPayrollResultsByPayrollExport(payrollExport)).Return(payrollResultList);
				Expect.Call(assembler.DomainEntitiesToDtos(payrollResultList)).Return(new[] {dto});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPayrollResultStatusByExportIdQueryDto{PayrollExportId = payrollExport.Id.GetValueOrDefault()});
				result.First().Should().Be.EqualTo(dto);
			}
		}
	}
}