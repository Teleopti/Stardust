using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork.MessageSenders;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	[ScheduleDictionaryPersistTest]
	public class ScheduleDictionaryPersisterTest
	{
		public IScheduleDictionaryPersister Target;
		public IScheduleDictionaryPersistTestHelper Helper;
		public IPersonAssignmentRepository PersonAssignments;
		public InUnitOfWork InUnitOfWork;

		[Test]
		public void ShouldPersist()
		{
			var person = Helper.NewPerson();
			var schedules = Helper.MakeDictionary();
			schedules[person]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();

			Target.Persist(schedules);

			InUnitOfWork.Do(() => PersonAssignments.LoadAll().Single().Person.Should().Be(person));
		}

	}
}