using System;
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
	public class GetPayrollResultStatusByIdQueryHandlerTest
	{
		private MockRepository mocks;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetPayrollResultStatusByIdQueryHandler target;
		private IAssembler<IPayrollResult, PayrollResultDto> assembler;
		private IPayrollResultRepository payrollResultRepository;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			payrollResultRepository = mocks.StrictMock<IPayrollResultRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			assembler = mocks.DynamicMock<IAssembler<IPayrollResult, PayrollResultDto>>();
			target = new GetPayrollResultStatusByIdQueryHandler(assembler, payrollResultRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetPayrollResultById()
		{
			var payrollExport = new PayrollExport();
			var payrollResult = new PayrollResult(payrollExport, null, DateTime.UtcNow);
			payrollResult.SetId(Guid.NewGuid());
			var dto = new PayrollResultDto();
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			using (mocks.Record())
			{
				Expect.Call(payrollResultRepository.Load(payrollResult.Id.GetValueOrDefault())).Return(payrollResult);
				Expect.Call(assembler.DomainEntityToDto(payrollResult)).Return(dto);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPayrollResultStatusByIdQueryDto{PayrollResultId = payrollResult.Id.GetValueOrDefault()});
				result.First().Should().Be.EqualTo(dto);
			}
		}
	}
}