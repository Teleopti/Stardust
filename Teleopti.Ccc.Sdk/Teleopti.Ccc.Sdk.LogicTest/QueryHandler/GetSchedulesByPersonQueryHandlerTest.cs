﻿using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetSchedulesByPersonQueryHandlerTest
	{
		private MockRepository mocks;
		private IScheduleRepository scheduleRepository;
        private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private GetSchedulesByPersonQueryHandler target;
		private IDateTimePeriodAssembler dateTimePeriodAssembler;
		private ISchedulePartAssembler scheduleDayAssembler;
		private IPersonRepository personRepository;
		private IScenarioRepository scenarioRepository;
		private Guid scenarioId;
		private Guid person1Id;
		private IUnitOfWork unitOfWork;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleRepository = mocks.DynamicMock<IScheduleRepository>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			dateTimePeriodAssembler = mocks.DynamicMock<IDateTimePeriodAssembler>();
			scheduleDayAssembler = mocks.DynamicMock<ISchedulePartAssembler>();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
            unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			
			scenarioId = Guid.NewGuid();
			person1Id = Guid.NewGuid();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			
			target = new GetSchedulesByPersonQueryHandler(unitOfWorkFactory,scheduleRepository,personRepository,scenarioRepository,dateTimePeriodAssembler,scheduleDayAssembler);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldGetScheduleForTeamInGivenScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dictionary = mocks.DynamicMock<IScheduleDictionary>();
			var person1 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(scenario);
				Expect.Call(personRepository.Get(person1Id)).IgnoreArguments().Return(person1);
				Expect.Call(scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
			}
			using (mocks.Playback())
			{
				var result =
					target.Handle(new GetSchedulesByPersonQueryDto
					              	{
					              		StartDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
					              		EndDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
					              		ScenarioId = scenarioId,
					              		PersonId = person1Id,
					              		TimeZoneId = "W. Europe Standard Time",
					              		SpecialProjection = "special"
					              	});
				result.Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldNotGetScheduleForTeamUsingInvalidScenarioId()
		{
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(null);
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				Assert.Throws<FaultException>(()=>target.Handle(new GetSchedulesByPersonQueryDto
					{
						StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						ScenarioId = scenarioId,
						PersonId = person1Id,
						TimeZoneId = "W. Europe Standard Time",
						SpecialProjection = "special"
					}));
			}
		}

		[Test]
		public void ShouldGetScheduleForTeamInDefaultScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dictionary = mocks.DynamicMock<IScheduleDictionary>();
			var person1 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.LoadDefaultScenario()).Return(scenario);
				Expect.Call(personRepository.Get(person1Id)).IgnoreArguments().Return(person1);
				Expect.Call(scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
			}
			using (mocks.Playback())
			{
				var result =
					target.Handle(new GetSchedulesByPersonQueryDto
					{
						StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						ScenarioId = null,
						PersonId = person1Id,
						TimeZoneId = "W. Europe Standard Time",
						SpecialProjection = "special"
					});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}