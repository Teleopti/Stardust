using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.BugTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class Bug24079 : ScheduleScreenPersisterIntegrationTest
	{
		private PersonAssignment _personAssignment;

		protected override IPersistableScheduleData MakeScheduleData()
		{
			_personAssignment = new PersonAssignment(Person, Scenario, FirstDayDateOnly);
			_personAssignment.SetMainShiftLayers(new[] { new MainShiftLayer(Activity, FirstDayDateTimePeriod) }, ShiftCategory);
			return _personAssignment;
		}

		[Test]
		public void ShouldPersistWithPersonAssignmentKeyConflict()
		{
			MakeTarget();

			CreatePersonAssignmentAsAnotherUser(FirstDayDateOnly);

			var result = TryPersistScheduleScreen();

			result.Saved.Should().Be.False();
			result.ScheduleDictionaryConflicts.Should().Have.Count.EqualTo(1);
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new Repository(unitOfWork);
				var personAssignment = new PersonAssignmentRepository(unitOfWork).Get(_personAssignment.Id.Value);
				repository.Remove(personAssignment);
				unitOfWork.PersistAll();
			}
		}

		protected override IEnumerable<IAggregateRoot> TestDataToReassociate()
		{
			return new IAggregateRoot[] { };
		}

	}
}