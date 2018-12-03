using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetSchedulesBySiteQueryHandlerTest
	{
		private MockRepository mocks;
		private IScheduleStorage scheduleStorage;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetSchedulesBySiteQueryHandler target;
		private IDateTimePeriodAssembler dateTimePeriodAssembler;
		private ISchedulePartAssembler scheduleDayAssembler;
		private IPersonRepository personRepository;
		private IScenarioRepository scenarioRepository;
		private Guid scenarioId;
		private Guid siteId;
		private IUnitOfWork unitOfWork;
		private ISiteRepository siteRepository;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleStorage = mocks.DynamicMock<IScheduleStorage>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			dateTimePeriodAssembler = mocks.DynamicMock<IDateTimePeriodAssembler>();
			scheduleDayAssembler = mocks.DynamicMock<ISchedulePartAssembler>();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
			siteRepository = mocks.DynamicMock<ISiteRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			
			scenarioId = Guid.NewGuid();
			siteId = Guid.NewGuid();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			
			target = new GetSchedulesBySiteQueryHandler(currentUnitOfWorkFactory,scheduleStorage,personRepository,scenarioRepository,siteRepository,dateTimePeriodAssembler,scheduleDayAssembler);
		}

		[Test]
		public void ShouldGetScheduleForSiteInGivenScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dictionary = mocks.DynamicMock<IScheduleDictionary>();
			var team1 = TeamFactory.CreateSimpleTeam();
			var team2 = TeamFactory.CreateSimpleTeam();
			var site = SiteFactory.CreateSiteWithTeams(new[] {team1, team2});
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(scenario);
				Expect.Call(siteRepository.Get(siteId)).Return(site);
				Expect.Call(personRepository.FindPeopleBelongTeam(team1, new DateOnlyPeriod(2012,5,2,2012,5,2))).Return(new[] {person1});
				Expect.Call(personRepository.FindPeopleBelongTeam(team2, new DateOnlyPeriod(2012,5,2,2012,5,2))).Return(new[] {person2});
				Expect.Call(scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
				Expect.Call(dictionary[person2]).Return(scheduleRange);
				Expect.Call(scheduleDayAssembler.DomainEntitiesToDtos(null))
					.Return(new List<SchedulePartDto> { new SchedulePartDto() });
			}
			using (mocks.Playback())
			{
				var result =
					target.Handle(new GetSchedulesBySiteQueryDto
					              	{
					              		StartDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
					              		EndDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
					              		ScenarioId = scenarioId,
					              		SiteId = siteId,
					              		TimeZoneId = "W. Europe Standard Time",
					              		SpecialProjection = "special"
					              	});
				result.Count.Should().Be.EqualTo(2);
			}
		}

		[Test]
		public void ShouldNotGetScheduleForSiteUsingInvalidScenarioId()
		{
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(null);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				Assert.Throws<FaultException>(()=>target.Handle(new GetSchedulesBySiteQueryDto
					{
						StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						ScenarioId = scenarioId,
						SiteId = siteId,
						TimeZoneId = "W. Europe Standard Time",
						SpecialProjection = "special"
					}));
			}
		}

		[Test]
		public void ShouldGetScheduleForSiteInDefaultScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dictionary = mocks.DynamicMock<IScheduleDictionary>();
			var team1 = TeamFactory.CreateSimpleTeam();
			var team2 = TeamFactory.CreateSimpleTeam();
			var site = SiteFactory.CreateSiteWithTeams(new[] { team1, team2 });
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.LoadDefaultScenario()).Return(scenario);
				Expect.Call(siteRepository.Get(siteId)).Return(site);
				Expect.Call(personRepository.FindPeopleBelongTeam(team1, new DateOnlyPeriod(2012, 5, 2, 2012, 5, 2))).Return(new[] { person1 });
				Expect.Call(personRepository.FindPeopleBelongTeam(team2, new DateOnlyPeriod(2012, 5, 2, 2012, 5, 2))).Return(new[] { person2 });
				Expect.Call(scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
				Expect.Call(dictionary[person2]).Return(scheduleRange);
				Expect.Call(scheduleDayAssembler.DomainEntitiesToDtos(null))
					.Return(new List<SchedulePartDto> { new SchedulePartDto() });
			}
			using (mocks.Playback())
			{
				var result =
					target.Handle(new GetSchedulesBySiteQueryDto
					{
						StartDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						EndDate = new DateOnlyDto { DateTime = new DateTime(2012, 5, 2) },
						ScenarioId = null,
						SiteId = siteId,
						TimeZoneId = "W. Europe Standard Time",
						SpecialProjection = "special"
					});
				result.Count.Should().Be.EqualTo(2);
			}
		}
	}
}