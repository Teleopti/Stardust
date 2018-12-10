using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetSchedulesByGroupPageGroupQueryHandlerTest
	{
		private MockRepository mocks;
		private IScheduleStorage scheduleStorage;
		private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private GetSchedulesByGroupPageGroupQueryHandler target;
		private IDateTimePeriodAssembler dateTimePeriodAssembler;
		private ISchedulePartAssembler scheduleDayAssembler;
		private IPersonRepository personRepository;
		private IScenarioRepository scenarioRepository;
		private Guid scenarioId;
		private IUnitOfWork unitOfWork;
		private IGroupingReadOnlyRepository groupingReadOnlyRepository;
		private Guid groupPageGroupId;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleStorage = mocks.DynamicMock<IScheduleStorage>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			dateTimePeriodAssembler = mocks.DynamicMock<IDateTimePeriodAssembler>();
			scheduleDayAssembler = mocks.DynamicMock<ISchedulePartAssembler>();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();
			groupingReadOnlyRepository = mocks.DynamicMock<IGroupingReadOnlyRepository>();
			unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			
			scenarioId = Guid.NewGuid();
			groupPageGroupId = Guid.NewGuid();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			
			target = new GetSchedulesByGroupPageGroupQueryHandler(unitOfWorkFactory,scheduleStorage,personRepository,scenarioRepository,groupingReadOnlyRepository,dateTimePeriodAssembler,scheduleDayAssembler);
		}

		[Test]
		public void ShouldGetScheduleForTeamInGivenScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dictionary = mocks.DynamicMock<IScheduleDictionary>();
			var person1Id = Guid.NewGuid();
			var person1 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(groupingReadOnlyRepository.DetailsForGroup(groupPageGroupId, new DateOnly(2012, 5, 2))).Return(
					new List<ReadOnlyGroupDetail> { new ReadOnlyGroupDetail { PersonId = person1Id} });
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(scenario);
				Expect.Call(personRepository.FindPeople((IEnumerable<Guid>)null)).Constraints(Rhino.Mocks.Constraints.List.Equal(new[] { person1Id })).Return(new[] { person1 });
				Expect.Call(scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
				Expect.Call(scheduleDayAssembler.DomainEntitiesToDtos(null))
					.Return(new List<SchedulePartDto> { new SchedulePartDto() });
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					var result =
						target.Handle(new GetSchedulesByGroupPageGroupQueryDto
						{
							QueryDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
							ScenarioId = scenarioId,
							TimeZoneId = "W. Europe Standard Time",
							GroupPageGroupId = groupPageGroupId
						});
					result.Count.Should().Be.EqualTo(1);
				}
			}
		}

		[Test]
		public void ShouldNotGetScheduleForTeamUsingInvalidScenarioId()
		{
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.Get(scenarioId)).Return(null);
				Expect.Call(unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					Assert.Throws<FaultException>(() => target.Handle(new GetSchedulesByGroupPageGroupQueryDto
					{
						QueryDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
						ScenarioId = scenarioId,
						GroupPageGroupId = groupPageGroupId,
						TimeZoneId = "W. Europe Standard Time"
					}));
				}
			}
		}

		[Test]
		public void ShouldGetScheduleForTeamInDefaultScenario()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dictionary = mocks.DynamicMock<IScheduleDictionary>();
			var person1Id = Guid.NewGuid();
			var person1 = PersonFactory.CreatePerson();
			var scheduleRange = mocks.DynamicMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.LoadDefaultScenario()).Return(scenario);
				Expect.Call(groupingReadOnlyRepository.DetailsForGroup(groupPageGroupId, new DateOnly(2012, 5, 2))).Return(
					new List<ReadOnlyGroupDetail> { new ReadOnlyGroupDetail { PersonId = person1Id } });
				Expect.Call(personRepository.FindPeople((IEnumerable<Guid>)null)).Constraints(Rhino.Mocks.Constraints.List.Equal(new[] { person1Id })).Return(new[] { person1 });
				Expect.Call(scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), scenario)).IgnoreArguments().Return(dictionary);
				Expect.Call(unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(dictionary[person1]).Return(scheduleRange);
				Expect.Call(scheduleDayAssembler.DomainEntitiesToDtos(null))
					.Return(new List<SchedulePartDto> {new SchedulePartDto()});
			}

			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					var result =
						target.Handle(new GetSchedulesByGroupPageGroupQueryDto
						{
							QueryDate = new DateOnlyDto {DateTime = new DateTime(2012, 5, 2)},
							ScenarioId = null,
							GroupPageGroupId = groupPageGroupId,
							TimeZoneId = "W. Europe Standard Time"
						});
					result.Count.Should().Be.EqualTo(1);
				}
			}
		}
	}
}