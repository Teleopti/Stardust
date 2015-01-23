using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.PerformanceTool;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.PerformanceTool
{
	[TestFixture]
	[Category("LongRunning")]
	public class ScheduleGeneratorTest : DatabaseTest
	{
		[Test]
		public void ShouldGenerateScheduleForAPerson()
		{
			var date = new DateOnly(2015, 1, 1);
			
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var personAssignmentRepository = new PersonAssignmentRepository(unitOfWork);
			var scenarioRepository = new ScenarioRepository(unitOfWork);
			var personRepository = new PersonRepository(unitOfWork);
			var scheduleRepository = new ScheduleRepository(unitOfWork);
			var shiftCategoryRepository = new ShiftCategoryRepository(unitOfWork);
			var personAbsenceAccountRepository = new PersonAbsenceAccountRepository(unitOfWork);
			var scheduleDifferenceSaver = new ScheduleDifferenceSaver(scheduleRepository);
			var schedulingResultStateHolder = new SchedulingResultStateHolder();
			var activityRepository = new ActivityRepository(unitOfWork);
			var scenario = new Scenario("default") {DefaultScenario = true};
			scenarioRepository.Add(scenario);
			var activity = new Activity("phone");
			activityRepository.Add(activity);
			var shiftCategory = new ShiftCategory("day");
			shiftCategoryRepository.Add(shiftCategory);
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			personRepository.Add(person);
			unitOfWork.Current().PersistAll();

			var target = new ScheduleGenerator(unitOfWork, scenarioRepository, personRepository, scheduleRepository,
				shiftCategoryRepository, personAbsenceAccountRepository, scheduleDifferenceSaver, schedulingResultStateHolder,
				activityRepository);
			target.Generate(person.Id.GetValueOrDefault(), date);

			var personAssignment = personAssignmentRepository.LoadAll().Single();
			personAssignment.ShiftLayers.Single().Payload.Should().Be(activity);
			personAssignment.ShiftCategory.Should().Be(shiftCategory);
		}
	}
}
