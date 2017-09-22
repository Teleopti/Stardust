using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetMultiplicatorDefinitionSetShiftAllowanceQueryHandlerTest
	{
		private IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IDateTimePeriodAssembler assembler;
		private GetMultiplicatorDefinitionSetShiftAllowanceQueryHandler target;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
		private IUnitOfWork unitOfWork;

		[SetUp]
		public void Setup()
		{
			multiplicatorDefinitionSetRepository = MockRepository.GenerateMock<IMultiplicatorDefinitionSetRepository>();
			unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			assembler = MockRepository.GenerateMock<IDateTimePeriodAssembler>();
			target = new GetMultiplicatorDefinitionSetShiftAllowanceQueryHandler(multiplicatorDefinitionSetRepository, assembler, currentUnitOfWorkFactory);
			unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
		}

		[Test]
		public void ShouldGetMultiplicatorDefinitionSetForShiftAllowance()
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Shift Allowance", MultiplicatorType.OBTime);
			var multiplicatorDefinitionSetList = new List<IMultiplicatorDefinitionSet> {multiplicatorDefinitionSet};

			multiplicatorDefinitionSetRepository.Stub(x => x.FindAllShiftAllowanceDefinitions())
			                                    .Return(multiplicatorDefinitionSetList);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);

			var multiplicatorDefinitionSetShiftAllowanceDto = new GetMultiplicatorDefinitionSetShiftAllowanceDto
				{
					Period = new DateOnlyPeriodDto
						{
							StartDate = new DateOnlyDto {DateTime = new DateTime(2012, 9, 19)},
							EndDate = new DateOnlyDto {DateTime = new DateTime(2012, 9, 19)}
						},
					TimeZoneId = TimeZoneInfo.Local.Id
				};
			var result = target.Handle(multiplicatorDefinitionSetShiftAllowanceDto);
			var first = result.FirstOrDefault();
			Assert.IsNotNull(first);
			Assert.AreEqual(first.Name, "Shift Allowance");
			Assert.IsFalse(first.IsDeleted);
			Assert.AreEqual(first.LayerCollection.Count, 0);
			unitOfWork.AssertWasNotCalled(x => x.DisableFilter(QueryFilter.Deleted));
		}

		[Test]
		public void ShouldGetDeletedMultiplicatorDefinitionSetForShiftAllowance()
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Shift Allowance", MultiplicatorType.OBTime);
			multiplicatorDefinitionSet.SetDeleted();
			var multiplicatorDefinitionSetList = new List<IMultiplicatorDefinitionSet> { multiplicatorDefinitionSet };

			multiplicatorDefinitionSetRepository.Stub(x => x.FindAllShiftAllowanceDefinitions())
												.Return(multiplicatorDefinitionSetList);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);

			var multiplicatorDefinitionSetShiftAllowanceDto = new GetMultiplicatorDefinitionSetShiftAllowanceDto
			{
				LoadDeleted = true,
				Period = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 9, 19) },
					EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 9, 19) }
				},
				TimeZoneId = TimeZoneInfo.Local.Id
			};
			var result = target.Handle(multiplicatorDefinitionSetShiftAllowanceDto);
			var first = result.FirstOrDefault();
			Assert.IsNotNull(first);
			Assert.IsTrue(first.IsDeleted);
			unitOfWork.AssertWasCalled(x => x.DisableFilter(QueryFilter.Deleted));
		}
	}
}
