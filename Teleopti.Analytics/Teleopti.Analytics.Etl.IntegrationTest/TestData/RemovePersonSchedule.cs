using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public static class RemovePersonSchedule
	{
		public static void RemoveAssignmentAndReadmodel(IScenario scenario, string name, DateTime dateTime, IPerson person)
		{
		var remove = new DeletePersonAssignment(scenario, new DateOnly(dateTime));
			Data.Person(name).Apply(remove);

			var readModel = new ScheduleDayReadModel
				{
					PersonId = person.Id.GetValueOrDefault(),
					ColorCode = 0,
					ContractTimeTicks = 0,
					Date = dateTime,
					StartDateTime = dateTime,
					EndDateTime = dateTime,
					Label = "LABEL",
					NotScheduled = true,
					Workday = false,
					WorkTimeTicks = 0
				};
			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
		}
	}
}
