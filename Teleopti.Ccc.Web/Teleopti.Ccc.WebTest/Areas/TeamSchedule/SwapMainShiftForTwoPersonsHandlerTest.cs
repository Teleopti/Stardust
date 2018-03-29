using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture]
	[TeamScheduleTest]
	public class SwapMainShiftForTwoPersonsCommandHandlerTest : ISetup
	{
		public SwapMainShiftForTwoPersonsCommandHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeActivityRepository ActivityRepository;
		
		private readonly DateTime scheduleDate = new DateTime(2016, 01, 01);
		
		[Test]
		public void ShouldSwapShiftsAndPersistSchedules()
		{
			Guid personIdFrom = Guid.NewGuid();
			Guid personIdTo = Guid.NewGuid();

			var personFrom = PersonFactory.CreatePerson().WithId(personIdFrom);
			personFrom.WithName(new Name("Abc", "123"));

			var personTo = PersonFactory.CreatePerson().WithId(personIdTo);

			PersonRepository.Add(personFrom);
			PersonRepository.Add(personTo);

			var defaultScenario = ScenarioRepository.Has("Default");

			var act = ActivityRepository.Has("Phone");
			PersonAssignmentRepository.Has(personFrom, defaultScenario, act, new ShiftCategory(), new DateOnly(scheduleDate),
				new TimePeriod(10, 11));

			var result = Target.SwapShifts(new SwapMainShiftForTwoPersonsCommand
			{
				PersonIdFrom = personIdFrom,
				PersonIdTo = personIdTo,
				ScheduleDate = scheduleDate
			});

			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnErrorsWhenBrokenBusinessRules()
		{
			Guid personIdFrom = Guid.NewGuid();
			Guid personIdTo = Guid.NewGuid();

			var personFrom = PersonFactory.CreatePerson().WithId(personIdFrom);
			personFrom.WithName(new Name("Abc", "123"));

			var personTo = PersonFactory.CreatePerson().WithId(personIdTo);
			personTo.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());

			PersonRepository.Add(personFrom);
			PersonRepository.Add(personTo);

			var defaultScenario = ScenarioRepository.Has("Default");

			var act = ActivityRepository.Has("Phone");
			var shiftCategory = new ShiftCategory();
			var date = new DateOnly(scheduleDate);
			PersonAssignmentRepository.Has(personTo, defaultScenario, act, shiftCategory, date,
				new TimePeriod(3, 9));
			PersonAssignmentRepository.Has(personFrom, defaultScenario, act, shiftCategory, date,
				new TimePeriod(3, 8));

			var result = Target.SwapShifts(new SwapMainShiftForTwoPersonsCommand
			{
				PersonIdFrom = personIdFrom,
				PersonIdTo = personIdTo,
				ScheduleDate = scheduleDate
			}).ToList();

			result.Count.Should().Be.EqualTo(1);
			var error = result[0];

			error.PersonId.Should().Be.EqualTo(personIdFrom);
			error.ErrorMessages.Count.Should().Be.EqualTo(1);
			error.ErrorMessages[0].Should().Not.Be.Empty();
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<SwapMainShiftForTwoPersonsCommandHandler>();
		}
	}
}
