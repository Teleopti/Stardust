﻿using System.Collections.Generic;
using NHibernate.Stat;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules.Performance
{
	[Ignore("Fix #47649")]
	public class NoUnnecessaryUpdatesWhenAddingNewAssignment : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2001, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			yield break;
		}
		
		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Add(new PersonAssignment(Person, Scenario, date)
				.WithLayer(Activity, new TimePeriod(8, 17))
				.WithLayer(Activity, new TimePeriod(9, 18)));
			DoModify(day);
		}

		protected override void Then(IStatistics statistics)
		{
			//only assignment should be updated
			statistics.EntityUpdateCount.Should().Be.EqualTo(0);
		}
	}
}