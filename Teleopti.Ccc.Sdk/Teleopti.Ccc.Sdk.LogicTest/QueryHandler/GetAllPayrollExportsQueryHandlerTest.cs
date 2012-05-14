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
	public class GetAllPayrollExportsQueryHandlerTest
	{
		private MockRepository mocks;
		private IPayrollExportRepository payrollExportRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetAllPayrollExportsQueryHandler target;
		private IAssembler<IPayrollExport, PayrollExportDto> assembler;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			payrollExportRepository = mocks.DynamicMock<IPayrollExportRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			assembler = mocks.DynamicMock<IAssembler<IPayrollExport, PayrollExportDto>>();
			target = new GetAllPayrollExportsQueryHandler(assembler, payrollExportRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetAllPayrollExports()
		{
			var payrollExport = new PayrollExport();
			var payrollExportList = new List<IPayrollExport> {payrollExport};
			var dto = new PayrollExportDto();
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			using (mocks.Record())
			{
				Expect.Call(payrollExportRepository.LoadAll()).Return(payrollExportList);
				Expect.Call(assembler.DomainEntitiesToDtos(payrollExportList)).Return(new[] {dto});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetAllPayrollExportsQueryDto());
				result.First().Should().Be.EqualTo(dto);
			}
		}
	}
}