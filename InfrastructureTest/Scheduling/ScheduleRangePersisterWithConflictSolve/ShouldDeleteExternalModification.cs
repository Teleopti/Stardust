using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling.ScheduleRangePersisterWithConflictSolve
{
	[TestFixture]
	public class ShouldDeleteExternalModification : ScheduleRangePersisterWithConflictSolveBase
	{
		private PersonAssignment _personAssignment;

		protected override void CreateBaseSchedules()
		{
			_personAssignment = new PersonAssignment(Agent, Scenario, StartDate);
			PersonAssignmentRepository.Add(_personAssignment);
		}

		protected override void ModifyFirst(IScheduleDictionary scheduleDictionary)
		{
			var scheduleDay = scheduleDictionary[Agent].ScheduledDay(StartDate);
			scheduleDay.CreateAndAddDayOff(DayOffTemplate);
			DoModify(scheduleDay);
		}

		protected override void ModifySecond(IScheduleDictionary scheduleDictionary)
		{
			var scheduleDay = scheduleDictionary[Agent].ScheduledDay(StartDate);
			scheduleDay.Remove(_personAssignment);
			DoModify(scheduleDay);
		}

		protected override void VerifyResult(IScheduleDictionary result)
		{
			var scheduleDay = result[Agent].ScheduledDay(StartDate);
			var assignment = scheduleDay.PersistableScheduleDataCollection().FirstOrDefault() as IPersonAssignment;
			assignment.Should().Be.Null();
		}
	}
}