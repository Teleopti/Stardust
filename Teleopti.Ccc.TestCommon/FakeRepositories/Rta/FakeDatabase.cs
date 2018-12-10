using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class StateForTest
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public DateTime? SnapshotId { get; set; }

		public StateForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			SourceId = "sourceId";
			UserCode = "8808";
			StateCode = "AUX2";
		}
	}

	public class CloseSnapshotForTest
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public DateTime SnapshotId { get; set; }

		public CloseSnapshotForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			SourceId = "sourceId";
		}
	}

	public class BatchForTest : BatchInputModel
	{
		public BatchForTest()
		{
			AuthenticationKey = LegacyAuthenticationKey.TheKey;
			SourceId = "sourceId";
		}
	}

	public class BatchStateForTest : BatchStateInputModel
	{
		public BatchStateForTest()
		{
			UserCode = "8808";
			StateCode = "AUX2";
		}
	}
	
	public static class FakeDatabaseScheduleExtensions
	{
		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, null, null);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, name, null);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string name, string belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, belongsToDate, null, Color.Black);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, string name, Guid activityId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, name, color);
		}
		
		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, start, end, null, null, color);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Color color, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, null, color);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, Guid.NewGuid(), start, end, null, name, null);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase database, Guid personId, string start, string end)
		{
			return database.WithSchedule(personId, Guid.NewGuid(), start, end, null, null, null);
		}

		public static FakeDatabase WithSchedule(this FakeDatabase fakeDataBuilder, Guid personId, Guid activityId, string start, string end, string belongsToDate, string name, Color? color)
		{
			fakeDataBuilder.WithPerson(personId, null);
			fakeDataBuilder.WithAssignment(personId, belongsToDate ?? start);
			fakeDataBuilder.WithActivity(activityId, name, color);
			fakeDataBuilder.WithAssignedActivity(start, end);
			return fakeDataBuilder;
		}
	}
}