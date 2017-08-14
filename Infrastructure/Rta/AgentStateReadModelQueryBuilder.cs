using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class Selection
	{
		public string Query { get; set; }
		public IEnumerable<Func<ISQLQuery, IQuery>> ParameterFuncs { get; set; }
	}

	public class AgentStateReadModelQueryBuilder
	{
		private readonly SelectionInfos _selections = new SelectionInfos();
		private readonly INow _now;
		private readonly ICurrentBusinessUnit _businessUnit;
		private Guid?[] excluded;

		public AgentStateReadModelQueryBuilder(INow now, ICurrentBusinessUnit businessUnit)
		{
			_now = now;
			_businessUnit = businessUnit;
		}

		public AgentStateReadModelQueryBuilder WithSelection(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			if (siteIds != null)
				_selections.Add(new SelectionInfo
				{
					Query = "a.SiteId IN (:siteIds)",
					Set = s => s.SetParameterList("siteIds", siteIds),
					SelectionType = SelectionType.Org
				});
			if (teamIds != null)
				_selections.Add(new SelectionInfo
				{
					Query = "a.TeamId IN (:teamIds)",
					Set = s => s.SetParameterList("teamIds", teamIds),
					SelectionType = SelectionType.Org
				});
			if (skillIds != null)
				_selections.Add(new SelectionInfo
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
					SelectionType = SelectionType.Skill
				});
			return this;
		}

		public AgentStateReadModelQueryBuilder InAlarm()
		{
			_selections.Add(new SelectionInfo
			{
				Query = @" 
AlarmStartTime <= :now 
ORDER BY AlarmStartTime ASC ",
				Set = s => s.SetParameter("now", _now.UtcDateTime()),
				SelectionType = SelectionType.Alarm
			});
			return this;
		}

		public AgentStateReadModelQueryBuilder Exclude(IEnumerable<Guid?> excludedStates)
		{
			excluded = excludedStates.ToArray();
			if (excluded.All(x => x.HasValue))
				_selections.Add(new SelectionInfo
				{
					Query = "StateGroupId NOT IN(:excludedStateGroupIds) OR StateGroupId IS NULL ",
					Set = s => s.SetParameterList("excludedStateGroupIds", excluded.Where(x => x.HasValue)),
					SelectionType = SelectionType.ExcludeStateGroups
				});
			else if (excluded.Any(x => x.HasValue))
				_selections.Add(new SelectionInfo
				{
					Query = "StateGroupId NOT IN(:excludedStateGroupIds) ",
					Set = s => s.SetParameterList("excludedStateGroupIds", excluded.Where(x => x.HasValue)),
					SelectionType = SelectionType.ExcludeStateGroups
				});
			else
				_selections.Add(new SelectionInfo
				{
					Query = "StateGroupId IS NOT NULL ",
					Set = s => s,
					SelectionType = SelectionType.ExcludeStateGroups
				});
			return this;
		}

		public Selection Build()
		{
			var builder = new StringBuilder("SELECT DISTINCT TOP 50 a.* FROM [ReadModel].AgentState a WITH (NOLOCK) ");



			if (_selections.Any(SelectionType.Skill))
			{
				builder.Append(_selections.QueryFor(SelectionType.Skill).Single());
				if (_selections.Any(SelectionType.Org))
					builder
						.Append(" AND a.IsDeleted = 0 AND ")
						.Append("(" + string.Join(" OR ", _selections.QueryFor(SelectionType.Org)) + ")");
			}
			else
			{
				builder
					.Append(" WHERE a.IsDeleted = 0 ");
				if(_selections.Any(SelectionType.Org))
					builder.Append("AND (" + string.Join(" OR ", _selections.QueryFor(SelectionType.Org)) + ")");
			}
				

			if (_selections.Any(SelectionType.ExcludeStateGroups))
				builder
					.Append(" AND ")
					.Append("(" + _selections.QueryFor(SelectionType.ExcludeStateGroups).Single() + ")");

			builder.Append(" AND a.BusinessUnitId = :BusinessUnitId ");

			if (_selections.Any(SelectionType.Alarm))
				builder
					.Append(" AND ")
					.Append(_selections.QueryFor(SelectionType.Alarm).Single());

			return new Selection
			{
				Query = builder.ToString(),
				ParameterFuncs = _selections.ParameterFuncs()
					.Append(s => s.SetGuid("BusinessUnitId", _businessUnit.Current().Id.Value))
					.ToArray()
			};
		}
	}
}