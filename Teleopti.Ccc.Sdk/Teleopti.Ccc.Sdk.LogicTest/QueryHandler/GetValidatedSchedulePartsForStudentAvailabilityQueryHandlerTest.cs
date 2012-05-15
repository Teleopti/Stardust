﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
	public class GetValidatedSchedulePartsForStudentAvailabilityQueryHandlerTest
	{
		private MockRepository mocks;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private GetValidatedSchedulePartsForStudentAvailabilityQueryHandler target;
		private IShiftCategoryRepository shiftCategoryRepository;
		private IActivityRepository activityRepository;
		private IPersonRepository personRepository;
		private IScheduleRepository scheduleRepository;
		private IScenarioProvider scenarioProvider;
		private IAssembler<IPreferenceDay, PreferenceRestrictionDto> preferenceDayAssembler;
		private IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> studentAvailabilityDayAssembler;
		private IRuleSetProjectionService ruleSetProjectionService;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			shiftCategoryRepository = mocks.DynamicMock<IShiftCategoryRepository>();
			activityRepository = mocks.DynamicMock<IActivityRepository>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			scheduleRepository = mocks.DynamicMock<IScheduleRepository>();
			scenarioProvider = mocks.DynamicMock<IScenarioProvider>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			preferenceDayAssembler = mocks.DynamicMock<IAssembler<IPreferenceDay, PreferenceRestrictionDto>>();
			studentAvailabilityDayAssembler = mocks.DynamicMock<IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto>>();
			ruleSetProjectionService = mocks.DynamicMock<IRuleSetProjectionService>();

			target = new GetValidatedSchedulePartsForStudentAvailabilityQueryHandler(unitOfWorkFactory, shiftCategoryRepository, activityRepository, personRepository, scheduleRepository, scenarioProvider, preferenceDayAssembler, studentAvailabilityDayAssembler, ruleSetProjectionService);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldGetValidatedSchedulePartsForStudentAvailability()
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
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(scheduleRepository.FindSchedulesOnlyInGivenPeriod(null, null, new DateTimePeriod(), null)).
					IgnoreArguments().Return(dictionary);
				Expect.Call(dictionary[person]).Return(range).Repeat.AtLeastOnce();
				Expect.Call(range.ScheduledDay(date)).IgnoreArguments().Return(scheduleDay).Repeat.AtLeastOnce();
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetValidatedSchedulePartsForStudentAvailabilityQueryDto { DateInPeriod = new DateOnlyDto { DateTime = date }, Person = personDto, TimeZoneId = "W. Europe Standard Time" });
				result.Count.Should().Be.EqualTo(0);
			}
		}
	}
}
