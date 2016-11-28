using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Internal;
using NHibernate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelQueryBuilder
	{
		private readonly List<selectionInfo> selections = new List<selectionInfo>();
		private readonly INow _now;

		public AgentStateReadModelQueryBuilder(INow now)
		{
			_now = now;
		}

		public void InSites(IEnumerable<Guid> siteIds)
		{
			selections.Add(new selectionInfo
			{
				Query = "a.SiteId IN (:siteIds)",
				Set = s => s.SetParameterList("siteIds", siteIds),
				Type = Type.Org
			});
		}

		public void InTeams(IEnumerable<Guid> teamIds)
		{
			selections.Add(new selectionInfo
			{
				Query = "a.TeamId IN (:teamIds)",
				Set = s => s.SetParameterList("teamIds", teamIds),
				Type = Type.Org
			});
		}

		public void WithSkills(IEnumerable<Guid> skillIds)
		{
			selections.Add(new selectionInfo
			{
				Query = @"
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE g.PageId = :skillGroupingPageId	
AND g.GroupId IN (:SkillIds)
AND :today BETWEEN g.StartDate and g.EndDate",
				Set = s => s
					.SetParameterList("SkillIds", skillIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Id),
				Type = Type.Skill
			});
		}

		private bool inAlarm;
		public AgentStateReadModelQueryBuilder InAlarm()
		{
			inAlarm = true;
			return this;
		}

		private IEnumerable<Guid?> excluded;
		public AgentStateReadModelQueryBuilder Exclude(IEnumerable<Guid?> excludedStates)
		{
			excluded = excludedStates;
			return this;
		}

		public Selection Build()
		{
			var setFuncs = new List<Func<ISQLQuery, IQuery>>();
			var builder = new StringBuilder("SELECT DISTINCT TOP 50 a.* FROM [ReadModel].AgentState a WITH (NOLOCK) ");
			if (selections.All(x => x.Type == Type.Skill))
			{
				builder.Append(selections.Single().Query);
			} 
			else if (selections.All(x => x.Type == Type.Org))
			{
				builder
					.Append(" WHERE ( ")
					.Append(string.Join(" OR ", selections.Select(x => x.Query)))
					.Append(") ");
			}
			else
			{
				builder
					.Append(selections.Single(x => x.Type == Type.Skill).Query)
					.Append(" AND (")
					.Append(string.Join(" OR ", selections
						.Where(x => x.Type == Type.Org)
						.Select(x => x.Query)))
					.Append(")");

			}
			if (!excluded.IsNullOrEmpty())
			{
				builder
					.Append(" AND ")
					.Append("(");
				if (excluded.Any(x => x.HasValue))
				{
					builder.Append("StateGroupId NOT IN(:excludedStateGroupIds) ");
					setFuncs.Add(s => s.SetParameterList("excludedStateGroupIds", excluded.Where(x => x.HasValue)));
					if (excluded.All(x => x.HasValue))
						builder.Append("OR StateGroupId IS NULL ");
				}
				if (excluded.All(x => !x.HasValue))
					builder.Append("StateGroupId IS NOT NULL ");
				builder.Append(")");

			}
			if (inAlarm)
			{
				builder.Append(@" AND 
			AlarmStartTime <= :now 
			ORDER BY AlarmStartTime ASC ");
				setFuncs.Add(s => s.SetParameter("now", _now.UtcDateTime()));
			}

			return new Selection
			{
				Query = builder.ToString(),
				ParameterFuncs = setFuncs.Concat(selections.Select(x => x.Set)).ToArray()
			};
		}


		private class selectionInfo
		{
			public string Query { get; set; }
			public Func<ISQLQuery, IQuery> Set { get; set; }
			public Type Type { get; set; }
		}
		public enum Type
		{
			Skill = 0,
			Org = 1,
			Alarm
		}

	}

	public class Selection
	{
		public string Query { get; set; }
		public IEnumerable<Func<ISQLQuery, IQuery>> ParameterFuncs { get; set; }
	}
}