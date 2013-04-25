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
	public class GetMultiplicatorDefinitionSetOvertimeQueryHandlerTest
	{
		private MockRepository mocks;
		private IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IDateTimePeriodAssembler assembler;
		private GetMultiplicatorDefinitionSetOvertimeQueryHandler target;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			multiplicatorDefinitionSetRepository = mocks.DynamicMock<IMultiplicatorDefinitionSetRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			assembler = mocks.DynamicMock<IDateTimePeriodAssembler>();
			target = new GetMultiplicatorDefinitionSetOvertimeQueryHandler(multiplicatorDefinitionSetRepository, assembler, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetMultiplicatorDefinitionSetForOvertime()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var multiplicatorDefinitionSetList = new List<IMultiplicatorDefinitionSet> { multiplicatorDefinitionSet };

			using (mocks.Record())
			{
				Expect.Call(multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions()).Return(multiplicatorDefinitionSetList);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var multiplicatorDefinitionSetOvertimeDto = new GetMultiplicatorDefinitionSetOvertimeDto();
				multiplicatorDefinitionSetOvertimeDto.Period = new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto {DateTime = new DateTime(2012, 9, 12)},
						EndDate = new DateOnlyDto {DateTime = new DateTime(2012, 9, 19)}
					};
				multiplicatorDefinitionSetOvertimeDto.TimeZoneId = TimeZoneInfo.Local.Id;
				var result = target.Handle(multiplicatorDefinitionSetOvertimeDto);
				Assert.IsTrue(result.Count > 0);
				var first = result.ToList().ElementAt(0);
				Assert.AreEqual(first.Name, "Overtime");
				Assert.IsFalse(first.IsDeleted);
				Assert.AreEqual(first.LayerCollection.Count, 0);
			}
		}
	}
}
