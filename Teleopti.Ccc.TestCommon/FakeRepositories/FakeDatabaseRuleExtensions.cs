using System;
using System.Drawing;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabaseRuleExtensions
	{
		public static FakeDatabase WithMappedRule(this FakeDatabase database)
		{
			return database.WithMappedRule(null, "", null, 0, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode)
		{
			return database.WithMappedRule(null, stateCode, null, 0, stateCode, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, 0, null, null);
		}


		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, Adherence adherence)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, 0, null, adherence);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, int staffingEffect)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, staffingEffect, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, Guid? ruleId)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, Guid? ruleId, string name)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, name, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, string name)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, 0, name, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid activityId, int staffingEffect, Adherence adherence)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, staffingEffect, null, adherence);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, int staffingEffect, Adherence adherence)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, staffingEffect, null, adherence);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, Guid ruleId, string stateCode, Guid? activityId)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, null, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, Guid ruleId, string stateCode, Guid? activityId, string name)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, 0, name, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, Guid? ruleId, string stateCode, Guid? activityId, int staffingEffect, string name, Adherence? adherence)
		{
			return database.WithMappedRule(ruleId, stateCode, activityId, staffingEffect, name, adherence, null);
		}

		public static FakeDatabase WithMappedRule(this FakeDatabase database, string stateCode, Guid? activityId, int staffingEffect, string name, Adherence? adherence, Color color)
		{
			return database.WithMappedRule(Guid.NewGuid(), stateCode, activityId, staffingEffect, name, adherence, color);
		}
	}
}