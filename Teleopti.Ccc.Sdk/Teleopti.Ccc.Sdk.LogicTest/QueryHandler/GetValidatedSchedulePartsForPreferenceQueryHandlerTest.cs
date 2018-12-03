using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetValidatedSchedulePartsForPreferenceQueryHandlerTest
	{
		private MockRepository mocks;
		private ICurrentUnitOfWorkFactory unitOfWorkFactory;
		private GetValidatedSchedulePartsForPreferenceQueryHandler target;
		private IShiftCategoryRepository shiftCategoryRepository;
		private IActivityRepository activityRepository;
		private IPersonRepository personRepository;
		private IScheduleStorage scheduleStorage;
		private ICurrentScenario scenarioRepository;
		private IAssembler<IPreferenceDay, PreferenceRestrictionDto> preferenceDayAssembler;
		private IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> studentAvailabilityDayAssembler;
		private IWorkShiftWorkTime workShiftWorkTime;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			shiftCategoryRepository = mocks.DynamicMock<IShiftCategoryRepository>();
			activityRepository = mocks.DynamicMock<IActivityRepository>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			scheduleStorage = mocks.DynamicMock<IScheduleStorage>();
			scenarioRepository = mocks.DynamicMock<ICurrentScenario>();
			unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			preferenceDayAssembler = mocks.DynamicMock<IAssembler<IPreferenceDay, PreferenceRestrictionDto>>();
			studentAvailabilityDayAssembler = mocks.DynamicMock<IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto>>();
			workShiftWorkTime = mocks.DynamicMock<IWorkShiftWorkTime>();

			target = new GetValidatedSchedulePartsForPreferenceQueryHandler(unitOfWorkFactory,shiftCategoryRepository,activityRepository,personRepository,scheduleStorage,scenarioRepository,preferenceDayAssembler,studentAvailabilityDayAssembler,workShiftWorkTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test, SetCulture("sv-SE")]
		public void ShouldGetValidatedSchedulePartsForPreference()
		{
			var date = new DateOnly(2012, 4, 30);
			var personDto = new PersonDto {Id = Guid.NewGuid()};
			
			var person = PersonFactory.CreatePerson();
			PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person,date);

			var dayFactory = new SchedulePartFactoryForDomain(person, ScenarioFactory.CreateScenarioAggregate(),
			                                                  new DateTimePeriod(2012, 4, 30, 2012, 4, 30),
			                                                  SkillFactory.CreateSkill("Test"));
			var scheduleDay = dayFactory.CreatePartWithMainShift();

			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var dictionary = mocks.StrictMock<IScheduleDictionary>();
			var range = mocks.StrictMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(personRepository.Load(personDto.Id.GetValueOrDefault())).Return(person);
				Expect.Call(unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[person]).Return(range).Repeat.AtLeastOnce();
				Expect.Call(range.ScheduledDay(date)).IgnoreArguments().Return(scheduleDay).Repeat.AtLeastOnce();
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetValidatedSchedulePartsForPreferenceQueryDto{DateInPeriod = new DateOnlyDto{DateTime = date.Date},Person = personDto,TimeZoneId = "W. Europe Standard Time"});
				result.Count.Should().Be.EqualTo(16);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test, SetCulture("sv-SE")]
		public void ShouldGetValidatedSchedulePartsForPreferenceWithValidShiftBag()
		{
			var date = new DateOnly(2012, 4, 30);
			var personDto = new PersonDto { Id = Guid.NewGuid() };

			var person = PersonFactory.CreatePerson();
			PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, date);

			var dayFactory = new SchedulePartFactoryForDomain(person, ScenarioFactory.CreateScenarioAggregate(),
															  new DateTimePeriod(2012, 4, 30, 2012, 4, 30),
															  SkillFactory.CreateSkill("Test"));
			var scheduleDay = dayFactory.CreatePartWithMainShift();
			person.Period(date).RuleSetBag = new RuleSetBag();

			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var dictionary = mocks.StrictMock<IScheduleDictionary>();
			var range = mocks.StrictMock<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(personRepository.Load(personDto.Id.GetValueOrDefault())).Return(person);
				Expect.Call(unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null)).
					   IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[person]).Return(range).Repeat.AtLeastOnce();
				Expect.Call(range.ScheduledDay(date)).IgnoreArguments().Return(scheduleDay).Repeat.AtLeastOnce();
			}
			using (mocks.Playback())
			{
				var result =
					target.Handle(new GetValidatedSchedulePartsForPreferenceQueryDto
					{
						DateInPeriod = new DateOnlyDto { DateTime = date.Date },
						Person = personDto,
						TimeZoneId = "W. Europe Standard Time"
					});
				result.Count.Should().Be.EqualTo(16);
			}
		}
	}
}
