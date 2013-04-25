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
	public class GetSchedulesByTeamQueryHandlerTest
	{
		private MockRepository mocks;
		private IScheduleRepository scheduleRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetSchedulesByTeamQueryHandler target;
		private IDateTimePeriodAssembler dateTimePeriodAssembler;
		private ISchedulePartAssembler scheduleDayAssembler;
		private IPersonRepository personRepository;
		private IScenarioRepository scenarioRepository;
		private Guid scenarioId;
		private Guid teamId;
		private IUnitOfWork unitOfWork;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleRepository = mocks.DynamicMock<IScheduleRepository>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			dateTimePeriodAssembler = mocks.DynamicMock<IDateTimePeriodAssembler>();
			scheduleDayAssembler = mocks.DynamicMock<ISchedulePartAssembler>();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			
			scenarioId = Guid.NewGuid();
			teamId = Guid.NewGuid();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			
			target = new GetSchedulesByTeamQueryHandler(currentUnitOfWorkFactory,scheduleRepository,personRepository,scenarioRepository,dateTimePeriodAssembler,scheduleDayAssembler);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldGetScheduleForTeamInGivenScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dictionary = mocks.DynamicMock<IScheduleDictionary>();
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(scenario);
				Expect.Call(personRepository.FindPeopleBelongTeam(null, new DateOnlyPeriod())).IgnoreArguments().Return(new[] {person1, person2});
				Expect.Call(scheduleRepository.FindSchedulesOnlyInGivenPeriod(null,null,new DateTimePeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
				Expect.Call(dictionary[person2]).Return(scheduleRange);
			}
			using (mocks.Playback())
			{
				var result =
					target.Handle(new GetSchedulesByTeamQueryDto
					              	{
					              		StartDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
					              		EndDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
					              		ScenarioId = scenarioId,
					              		TeamId = teamId,
					              		TimeZoneId = "W. Europe Standard Time",
					              		SpecialProjection = "special"
					              	});
				result.Count.Should().Be.EqualTo(2);
			}
		}

		[Test]
		public void ShouldNotGetScheduleForTeamUsingInvalidScenarioId()
		{
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(null);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				Assert.Throws<FaultException>(()=>target.Handle(new GetSchedulesByTeamQueryDto
					{
						StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						ScenarioId = scenarioId,
						TeamId = teamId,
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
			var person2 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.LoadDefaultScenario()).Return(scenario);
				Expect.Call(personRepository.FindPeopleBelongTeam(null, new DateOnlyPeriod())).IgnoreArguments().Return(new[] { person1, person2 });
				Expect.Call(scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, new DateTimePeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
				Expect.Call(dictionary[person2]).Return(scheduleRange);
			}
			using (mocks.Playback())
			{
				var result =
					target.Handle(new GetSchedulesByTeamQueryDto
					{
						StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						ScenarioId = null,
						TeamId = teamId,
						TimeZoneId = "W. Europe Standard Time",
						SpecialProjection = "special"
					});
				result.Count.Should().Be.EqualTo(2);
			}
		}
	}
}