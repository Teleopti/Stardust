using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		private IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private IDifferenceCollectionService<IPersistableScheduleData> _differenceService;

		private ISwapMainShiftForTwoPersonsCommandHandler _target;

		private readonly Guid personIdFrom = Guid.NewGuid();
		private IPerson personFrom;

		private readonly Guid personIdTo = Guid.NewGuid();
		private IPerson personTo;
		private readonly DateTime scheduleDate = new DateTime(2016, 01, 01);
		
		[SetUp]
		public void SetUp()
		{
			personFrom = PersonFactory.CreatePerson();
			personFrom.SetId(personIdFrom);
			personFrom.Name = new Name("Abc", "123");

			personTo = PersonFactory.CreatePerson();
			personTo.SetId(personIdTo);

			_commonNameDescriptionSetting = new CommonNameDescriptionSetting();
			_personRepository = new FakePersonRepositoryLegacy(personFrom, personTo);

			var defaultScenario = new Scenario("Test Scenario")
			{
				DefaultScenario = true
			};
			defaultScenario.SetId(Guid.NewGuid());
			_scenarioRepository = new FakeScenarioRepository(defaultScenario);

			_scheduleRepository = new FakeScheduleRepository();
			_swapAndModifyServiceNew = MockRepository.GenerateMock<ISwapAndModifyServiceNew>();

			_scheduleDifferenceSaver = new FakeScheduleDifferenceSaver(_scheduleRepository);
			_differenceService = new DifferenceEntityCollectionService<IPersistableScheduleData>();
		}

		[Test, Ignore]
		public void ShouldSwapShiftsAndPersistSchedules()
		{
			_swapAndModifyServiceNew.Stub(x => x.Swap(personFrom, personTo, null, null, null, null, null))
				.Return(new List<BusinessRuleResponse>())
				.IgnoreArguments();

			_target = new SwapMainShiftForTwoPersonsCommandHandler(_commonNameDescriptionSetting, _personRepository,
				_scheduleRepository, _scenarioRepository, _swapAndModifyServiceNew, _differenceService, _scheduleDifferenceSaver);

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

			_target = new SwapMainShiftForTwoPersonsCommandHandler(_commonNameDescriptionSetting, _personRepository,
				_scheduleRepository, _scenarioRepository, _swapAndModifyServiceNew, _differenceService, _scheduleDifferenceSaver);

			var result = _target.SwapShifts(new SwapMainShiftForTwoPersonsCommand
			{
				PersonIdFrom = personIdFrom,
				PersonIdTo = personIdTo,
				ScheduleDate = scheduleDate
			}).ToList();

			result.Count.Should().Be.EqualTo(1);
			var error = result[0];

			error.PersonName.Should().Be.EqualTo("Abc 123");
			error.Message.Count.Should().Be.EqualTo(1);
			error.Message[0].Should().Be.EqualTo(errorMessage);
		}
	}
}
