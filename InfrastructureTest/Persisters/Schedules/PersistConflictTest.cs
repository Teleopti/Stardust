using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	public class PersistConflictTest
	{
		[Test]
		public void ShouldUseDatabaseVersionIfExist()
		{
			var expected = Guid.NewGuid();
			var target = new PersistConflict(
							new DifferenceCollectionItem<IPersistableScheduleData>(createScheduleDataWithId(Guid.NewGuid()), createScheduleDataWithId(Guid.NewGuid())), 
																				createScheduleDataWithId(expected));
			target.InvolvedId().Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldUseOrginalIfExistsAndNoDatabaseVersionExists()
		{
			var expected = Guid.NewGuid();
			var target = new PersistConflict(
						new DifferenceCollectionItem<IPersistableScheduleData>(createScheduleDataWithId(expected), createScheduleDataWithId(Guid.NewGuid())), null);
			target.InvolvedId().Should().Be.EqualTo(expected);
		}

		[Test]
		public void AnyOtherCaseShouldThrow()
		{
			var target = new PersistConflict(new DifferenceCollectionItem<IPersistableScheduleData>(), null);
			Assert.Throws<ArgumentException>(() => target.InvolvedId());
		}


		private static IPersistableScheduleData createScheduleDataWithId(Guid id)
		{
			var ret = new PersonAssignment(new Person(), new Scenario("s"), new DateOnly());
			ret.SetId(id);
			return ret;
		}
	}
}