using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class RemoveAbsenceCommandHandlerTest
	{
		[Test]
		public void ShouldRemoveAbsenceFromRepository()
		{
			var personAbsence = new PersonAbsence(MockRepository.GenerateMock<IScenario>());
			personAbsence.SetId(new Guid());

			var personAbsenceRepository = new TestWriteSideRepository<IPersonAbsence>() { personAbsence };
			
			var target = new RemoveAbsenceCommandHandler(personAbsenceRepository);

			var command = new RemoveAbsenceCommand
				{
					PersonAbsenceId = personAbsence.Id.Value
				};

			target.Handle(command);

			Assert.That(personAbsenceRepository.Any() == false); 
		}
	}
}