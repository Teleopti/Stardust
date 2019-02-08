using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabaseScheduleExtensions
	{
		public static FakeDatabase WithScenario(this FakeDatabase database, Guid? id)
		{
			return database.WithScenario(id, null);
		}

		public static FakeDatabase WithActivity(this FakeDatabase database)
		{
			return database.WithActivity(null, null, null);
		}

		public static FakeDatabase WithActivity(this FakeDatabase database, Guid? id)
		{
			return database.WithActivity(id, null, null);
		}

		public static FakeDatabase WithAssignedActivity(this FakeDatabase database, string startTime, string endTime)
		{
			return database.WithAssignedActivity(null, startTime, endTime);
		}

		public static FakeDatabase WithDayOffTemplate(this FakeDatabase database, Guid? id)
		{
			return database.WithDayOffTemplate(id, null, null);
		}

		public static FakeDatabase WithDayOffTemplate(this FakeDatabase database, string name, string shortName)
		{
			return database.WithDayOffTemplate(null, name, shortName);
		}

		public static FakeDatabase WithAbsence(this FakeDatabase database, Guid? id)
		{
			return database.WithAbsence(id, null, null);
		}

		public static FakeDatabase WithAbsence(this FakeDatabase database, string name)
		{
			return database.WithAbsence(null, name, null);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase database, string start, string end)
		{
			return database
				.WithAssignment(start)
				.WithAssignedActivity(start, end);
		}

		public static FakeDatabase WithSchedules(this FakeDatabase database, IEnumerable<DateTimePeriod> periods)
		{
			periods.ForEach(t => {
				database
					.WithAssignment(t.StartDateTime.ToString())
					.WithAssignedActivity(t.StartDateTime.ToString(), t.EndDateTime.ToString());
			});
			return database;
		}

		public static FakeDatabase WithScheduleDayOff(this FakeDatabase database, string date)
		{
			return database
				.WithAssignment(date)
				.WithDayOff();
		}
	}
}