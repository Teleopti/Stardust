﻿using System;
using System.Collections.Generic;
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

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			multiplicatorDefinitionSetRepository = mocks.DynamicMock<IMultiplicatorDefinitionSetRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			assembler = mocks.DynamicMock<IDateTimePeriodAssembler>();
			target = new GetMultiplicatorDefinitionSetOvertimeQueryHandler(multiplicatorDefinitionSetRepository, assembler, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetMultiplicatorDefinitionSetForShiftAllowance()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var multiplicatorDefinitionSetList = new List<IMultiplicatorDefinitionSet> { multiplicatorDefinitionSet };

			using (mocks.Record())
			{
				Expect.Call(multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions()).Return(
					multiplicatorDefinitionSetList);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var multiplicatorDefinitionSetOvertimeDto = new GetMultiplicatorDefinitionSetOvertimeDto();
				multiplicatorDefinitionSetOvertimeDto.Period = new DateOnlyPeriodDto(new DateOnlyPeriod(2012, 09, 19, 2012, 09, 19));
				multiplicatorDefinitionSetOvertimeDto.TimeZoneId = TimeZoneInfo.Local.Id;
				var result = target.Handle(multiplicatorDefinitionSetOvertimeDto);
				Assert.IsTrue(result.Count > 0);
			}
		}
	}
}
