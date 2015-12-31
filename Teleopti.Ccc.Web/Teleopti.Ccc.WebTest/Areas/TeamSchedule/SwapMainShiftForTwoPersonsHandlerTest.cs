using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture]
	internal class SwapMainShiftForTwoPersonsCommandHandlerTest
	{
		private ICommonNameDescriptionSetting _commonNameDescriptionSetting;
		private IPersonRepository _personRepository;
		private IScheduleRepository _scheduleRepository;
		private IScenarioRepository _scenarioRepository;
		private ISwapAndModifyServiceNew _swapAndModifyServiceNew;
		private IScheduleDictionaryPersister _scheduleDictionaryPersister;

		private ISwapMainShiftForTwoPersonsCommandHandler _target;

		private readonly Guid personIdFrom = Guid.NewGuid();
		private IPerson personFrom;

		private readonly Guid personIdTo = Guid.NewGuid();
		private IPerson personTo;
		private readonly DateTime scheduleDate = new DateTime(2016, 01, 01);
		private readonly IScheduleDictionary schedules = new FakeScheduleDictionary();

		[SetUp]
		public void SetUp()
		{
			personFrom = PersonFactory.CreatePerson();
			personFrom.SetId(personIdFrom);
			personFrom.Name = new Name("Abc", "123");

			personTo = PersonFactory.CreatePerson();
			personTo.SetId(personIdTo);
			personTo.Name = new Name("Def", "456");

			_commonNameDescriptionSetting = MockRepository.GenerateMock<ICommonNameDescriptionSetting>();
			_commonNameDescriptionSetting.Stub(x => x.BuildCommonNameDescription(personFrom))
				.Return(personFrom.Name.FirstName + "@" + personFrom.Name.LastName);

			var peoples = new List<IPerson> {personFrom, personTo};
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_personRepository.Stub(x => x.FindPeople(new[] {personIdFrom, personIdTo}))
				.Return(peoples);

			var defaultScenario = new Scenario("TestScenario");
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(defaultScenario);

			_scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
			var schedulePeriod = new ScheduleDateTimePeriod(period);
			var personProvider = new PersonProvider(_personRepository);
			var loadOptions = new ScheduleDictionaryLoadOptions(true, true);
			_scheduleRepository.Stub(
				x => x.FindSchedulesForPersons(schedulePeriod, defaultScenario, personProvider, loadOptions, peoples))
				.Return(schedules)
				.IgnoreArguments();

			_swapAndModifyServiceNew = MockRepository.GenerateMock<ISwapAndModifyServiceNew>();

			_scheduleDictionaryPersister = MockRepository.GenerateMock<IScheduleDictionaryPersister>();
		}

		[Test]
		public void ShouldSwapShiftsAndPersistSchedules()
		{
			_swapAndModifyServiceNew.Stub(x => x.Swap(personFrom, personTo, null, null, null, null, null))
				.Return(new List<BusinessRuleResponse>())
				.IgnoreArguments();
			_scheduleDictionaryPersister.Stub(x => x.Persist(schedules))
				.Return(new List<PersistConflict>())
				.IgnoreArguments();

			_target = new SwapMainShiftForTwoPersonsCommandHandler(_commonNameDescriptionSetting, _personRepository,
				_scheduleRepository, _scenarioRepository, _swapAndModifyServiceNew, _scheduleDictionaryPersister);

			var result = _target.SwapShifts(new SwapMainShiftForTwoPersonsCommand
			{
				PersonIdFrom = personIdFrom,
				PersonIdTo = personIdTo,
				ScheduleDate = scheduleDate
			});

			_swapAndModifyServiceNew.AssertWasCalled(x => x.Swap(
				Arg<IPerson>.Matches(a => (a.Id == personIdFrom)),
				Arg<IPerson>.Matches(a => (a.Id == personIdTo)),
				Arg<IList<DateOnly>>.Matches(a => (a.Count == 1 && a.Contains(new DateOnly(scheduleDate)))),
				Arg<IList<DateOnly>>.Matches(a => (a.Count == 0)),
				Arg<IScheduleDictionary>.Matches(a => (a != null)),
				Arg<INewBusinessRuleCollection>.Matches(a => (a != null)),
				Arg<IScheduleTagSetter>.Matches(a => (a != null))));

			_scheduleDictionaryPersister.AssertWasCalled(
				x => x.Persist(Arg<IScheduleDictionary>.Matches(a => (a != null))));

			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnErrorsWhenBrokenBusinessRules()
		{
			const string errorMessage = "Test error message";
			_swapAndModifyServiceNew.Stub(x => x.Swap(personFrom, personTo, null, null, null, null, null))
				.Return(new List<BusinessRuleResponse>
				{
					new BusinessRuleResponse(null, errorMessage, true, true, new DateTimePeriod(), personFrom,
						new DateOnlyPeriod())
				}).IgnoreArguments();
			_scheduleDictionaryPersister.Stub(x => x.Persist(schedules))
				.Return(new List<PersistConflict>())
				.IgnoreArguments();

			_target = new SwapMainShiftForTwoPersonsCommandHandler(_commonNameDescriptionSetting, _personRepository, _scheduleRepository,
				_scenarioRepository, _swapAndModifyServiceNew, _scheduleDictionaryPersister);

			var result = _target.SwapShifts(new SwapMainShiftForTwoPersonsCommand
			{
				PersonIdFrom = personIdFrom,
				PersonIdTo = personIdTo,
				ScheduleDate = scheduleDate
			}).ToList();

			result.Count.Should().Be.EqualTo(1);
			var error = result[0];

			error.PersonName.Should().Be.EqualTo("Abc@123");
			error.Message.Count.Should().Be.EqualTo(1);
			error.Message[0].Should().Be.EqualTo(errorMessage);
		}
	}
}
