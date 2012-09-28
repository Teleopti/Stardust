using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetMultiplicatorDefinitionSetShiftAllowanceQueryHandlerTest
	{

		private MockRepository mocks;
		private IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IDateTimePeriodAssembler assembler;
		private GetMultiplicatorDefinitionSetShiftAllowanceQueryHandler target;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			multiplicatorDefinitionSetRepository = mocks.DynamicMock<IMultiplicatorDefinitionSetRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			assembler = mocks.DynamicMock<IDateTimePeriodAssembler>();
			target = new GetMultiplicatorDefinitionSetShiftAllowanceQueryHandler(multiplicatorDefinitionSetRepository, assembler, unitOfWorkFactory);
		}
		
		[Test]
		public void ShouldGetMultiplicatorDefinitionSetForShiftAllowance()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Shift Allowance", MultiplicatorType.OBTime);
			var multiplicatorDefinitionSetList = new List<IMultiplicatorDefinitionSet> {multiplicatorDefinitionSet};

			using (mocks.Record())
			{
				Expect.Call(multiplicatorDefinitionSetRepository.FindAllShiftAllowanceDefinitions()).Return(
					multiplicatorDefinitionSetList);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var multiplicatorDefinitionSetShiftAllowanceDto = new GetMultiplicatorDefinitionSetShiftAllowanceDto();
				multiplicatorDefinitionSetShiftAllowanceDto.Period = new DateOnlyPeriodDto(new DateOnlyPeriod(2012,09,19,2012,09,19));
				multiplicatorDefinitionSetShiftAllowanceDto.TimeZoneId = TimeZoneInfo.Local.Id;
				var result = target.Handle(multiplicatorDefinitionSetShiftAllowanceDto);
				Assert.IsTrue(result.Count > 0);
				var first = result.ToList().ElementAt(0);
				Assert.AreEqual(first.Name, "Shift Allowance");
				Assert.IsFalse(first.IsDeleted);
				Assert.AreEqual(first.LayerCollection.Count, 0);

			}
		}

	}
}
