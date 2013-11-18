﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

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
							new DifferenceCollectionItem<INonversionedPersistableScheduleData>(createScheduleDataWithId(Guid.NewGuid()), createScheduleDataWithId(Guid.NewGuid())), 
																				createScheduleDataWithId(expected));
			target.InvolvedId().Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldUseOrginalIfExistsAndNoDatabaseVersionExists()
		{
			var expected = Guid.NewGuid();
			var target = new PersistConflict(
						new DifferenceCollectionItem<INonversionedPersistableScheduleData>(createScheduleDataWithId(expected), createScheduleDataWithId(Guid.NewGuid())), null);
			target.InvolvedId().Should().Be.EqualTo(expected);
		}

		[Test]
		public void AnyOtherCaseShouldThrow()
		{
			var target = new PersistConflict(new DifferenceCollectionItem<INonversionedPersistableScheduleData>(), null);
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