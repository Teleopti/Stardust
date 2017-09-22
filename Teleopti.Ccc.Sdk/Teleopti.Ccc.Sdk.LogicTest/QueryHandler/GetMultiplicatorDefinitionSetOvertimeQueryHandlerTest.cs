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
	public class GetMultiplicatorDefinitionSetOvertimeQueryHandlerTest
	{
		private IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IDateTimePeriodAssembler assembler;
		private GetMultiplicatorDefinitionSetOvertimeQueryHandler target;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
		private IUnitOfWork unitOfWork;

		[SetUp]
		public void Setup()
		{
			multiplicatorDefinitionSetRepository = MockRepository.GenerateMock<IMultiplicatorDefinitionSetRepository>();
			unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			assembler = MockRepository.GenerateMock<IDateTimePeriodAssembler>();
			target = new GetMultiplicatorDefinitionSetOvertimeQueryHandler(multiplicatorDefinitionSetRepository, assembler, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetMultiplicatorDefinitionSetForOvertime()
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var multiplicatorDefinitionSetList = new List<IMultiplicatorDefinitionSet> {multiplicatorDefinitionSet};

			multiplicatorDefinitionSetRepository.Stub(x => x.FindAllOvertimeDefinitions()).Return(multiplicatorDefinitionSetList);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);

			var multiplicatorDefinitionSetOvertimeDto = new GetMultiplicatorDefinitionSetOvertimeDto
				{
					Period = new DateOnlyPeriodDto
						{
							StartDate = new DateOnlyDto {DateTime = new DateTime(2012, 9, 12)},
							EndDate = new DateOnlyDto {DateTime = new DateTime(2012, 9, 19)}
						},
					TimeZoneId = TimeZoneInfo.Local.Id
				};
			var result = target.Handle(multiplicatorDefinitionSetOvertimeDto);
			var first = result.FirstOrDefault();
			Assert.IsNotNull(first);
			Assert.AreEqual(first.Name, "Overtime");
			Assert.IsFalse(first.IsDeleted);
			Assert.AreEqual(first.LayerCollection.Count, 0);
			unitOfWork.AssertWasNotCalled(x => x.DisableFilter(QueryFilter.Deleted));
		}

		[Test]
		public void ShouldGetDeletedMultiplicatorDefinitionSetForOvertime()
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			multiplicatorDefinitionSet.SetDeleted();
			var multiplicatorDefinitionSetList = new List<IMultiplicatorDefinitionSet> { multiplicatorDefinitionSet };

			multiplicatorDefinitionSetRepository.Stub(x => x.FindAllOvertimeDefinitions()).Return(multiplicatorDefinitionSetList);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);

			var multiplicatorDefinitionSetOvertimeDto = new GetMultiplicatorDefinitionSetOvertimeDto
			{
				LoadDeleted = true,
				Period = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 9, 12) },
					EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 9, 19) }
				},
				TimeZoneId = TimeZoneInfo.Local.Id
			};
			var result = target.Handle(multiplicatorDefinitionSetOvertimeDto);
			var first = result.FirstOrDefault();
			Assert.IsNotNull(first);
			Assert.IsTrue(first.IsDeleted);
			unitOfWork.AssertWasCalled(x => x.DisableFilter(QueryFilter.Deleted));
		}
	}
}
