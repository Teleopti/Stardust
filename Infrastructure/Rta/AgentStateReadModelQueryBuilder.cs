using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class Selection
	{
		public string Query { get; set; }
		public IEnumerable<Func<ISQLQuery, IQuery>> ParameterFuncs { get; set; }
	}

	public class AgentStateReadModelQueryBuilder
	{
		private readonly IList<string> _froms = new List<string>();
		private readonly IList<string> _wheres = new List<string>();
		private readonly IList<string> _orderbys = new List<string>();
		private readonly IList<Func<ISQLQuery, IQuery>> _parameters = new List<Func<ISQLQuery, IQuery>>();
		private int _topCount;

		public AgentStateReadModelQueryBuilder()
		{
			_topCount = 50;
		}

		public AgentStateReadModelQueryBuilder WithoutDeleted()
		{
			_wheres.Add("IsDeleted = 0");
			return this;
		}

		public AgentStateReadModelQueryBuilder WithBusinessUnit(Guid businessUnit)
		{
			_wheres.Add("a.BusinessUnitId = :BusinessUnitId");
			_parameters.Add(s => s.SetGuid("BusinessUnitId", businessUnit));
			return this;
		}

		public AgentStateReadModelQueryBuilder WithMax(int topCount)
		{
			_topCount = topCount;
			return this;
		}

		public AgentStateReadModelQueryBuilder WithPersons(IEnumerable<Guid> personIds)
		{
			_wheres.Add("a.PersonId IN (:personIds)");
			_parameters.Add(s => s.SetParameterList("personIds", personIds));
			return this;
		}

		public AgentStateReadModelQueryBuilder WithTextFilter(string textFilter)
		{
			if (!textFilter.IsNullOrEmpty())
			{
				textFilter.Split(null)
					.Select(x => x.Trim())
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Select((w, i) => new {word = w, index = i})
					.ForEach(x =>
					{
						_wheres.Add($"a.TextFilter LIKE :textFilter{x.index}");
						_parameters.Add(s => s.SetParameter($"textFilter{x.index}", $"%{x.word}%"));
					});
			}
			return this;
		}

		// put skills in its own With method probably ;)
		public AgentStateReadModelQueryBuilder WithSelection(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, INow now)
		{
			var teamSiteWheres = new List<string>();
			if (siteIds != null)
			{
				teamSiteWheres.Add("a.SiteId IN (:siteIds)");
				_parameters.Add(s => s.SetParameterList("siteIds", siteIds));
			}
			if (teamIds != null)
			{
				teamSiteWheres.Add("a.TeamId IN (:teamIds)");
				_parameters.Add(s => s.SetParameterList("teamIds", teamIds));
			}

			if (teamSiteWheres.Any())
				_wheres.Add($"({string.Join(" OR ", teamSiteWheres)})");

			if (skillIds != null)
			{
				_froms.Add("INNER JOIN ReadModel.GroupingReadOnly AS g ON a.PersonId = g.PersonId");
				_wheres.Add(@"g.PageId = :skillGroupingPageId	
AND g.GroupId IN (:SkillIds)
AND :today BETWEEN g.StartDate and g.EndDate");
				_parameters.Add(s => s
					.SetParameterList("SkillIds", skillIds)
					.SetParameter("today", now.UtcDateTime().Date)
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Id)
				);
			}
			return this;
		}

		public AgentStateReadModelQueryBuilder InAlarm(INow now)
		{
			_wheres.Add("AlarmStartTime <= :now ");
			_parameters.Add(s => s.SetParameter("now", now.UtcDateTime()));
			_orderbys.Add("AlarmStartTime ASC");
			return this;
		}

		public AgentStateReadModelQueryBuilder Exclude(IEnumerable<Guid?> excludedStates)
		{
			var excluded = excludedStates.ToArray();
			if (excluded.All(x => x.HasValue))
			{
				_wheres.Add("( StateGroupId NOT IN(:excludedStateGroupIds) OR StateGroupId IS NULL )");
				_parameters.Add(s => s.SetParameterList("excludedStateGroupIds", excluded.Where(x => x.HasValue)));
			}
			else if (excluded.Any(x => x.HasValue))
			{
				_wheres.Add("StateGroupId NOT IN(:excludedStateGroupIds)");
				_parameters.Add(s => s.SetParameterList("excludedStateGroupIds", excluded.Where(x => x.HasValue)));
			}
			else
			{
				_wheres.Add("StateGroupId IS NOT NULL ");
			}
			return this;
		}

		public Selection Build()
		{
			var builder = new StringBuilder($@"
				SELECT DISTINCT TOP {_topCount} 
					a.[PersonId], 
					a.[BusinessUnitId], 
					a.[SiteId], 
					a.[TeamId], 
					[ReceivedTime], 
					[Activity], 
					[NextActivity], 
					[NextActivityStartTime], 
					[StateName], 
					[StateStartTime], 
					[RuleName], 
					[RuleStartTime], 
					[RuleColor], 
					[StaffingEffect], 
					[IsRuleAlarm], 
					[AlarmStartTime], 
					[AlarmColor], 
					[Shift], 
					[OutOfAdherences], 
					[StateGroupId], 
					[IsDeleted], 
					a.[FirstName], 
					a.[LastName], 
					a.[EmploymentNumber], 
					[SiteName], 
					[TeamName] 
				FROM 
					[ReadModel].AgentState a WITH (NOLOCK)
				");

			_froms.ForEach(s => builder.Append(s));

			builder.Append(" WHERE ");
			builder.Append(string.Join(" AND ", _wheres));

			if (_orderbys.Any())
			{
				builder.Append(" ORDER BY ");
				builder.Append(string.Join(", ", _orderbys));
			}

			return new Selection
			{
				Query = builder.ToString(),
				ParameterFuncs = _parameters
			};
		}

		public AgentStateReadModelQueryBuilder WithSorting(ISorting sorting, string direction)
		{
			var ascending = sorting.AscendingFor(direction);
			sorting?.ColumnNames.ForEach(c => _orderbys.Add($"{c} {(ascending ? "" : "DESC")}"));
			return this;
		}

		public ISorting SortingFor(string orderBy) => _sortingByOrderBy[orderBy];

		public interface ISorting
		{
			IEnumerable<string> ColumnNames { get; }
			bool AscendingFor(string direction);
		}

		public class Sorting : ISorting
		{
			public Sorting(IEnumerable<string> columnNames)
			{
				ColumnNames = columnNames;
			}

			public IEnumerable<string> ColumnNames { get; }
			public virtual bool AscendingFor(string direction) => direction == "asc";
		}

		public class SortingInverted : Sorting
		{
			public SortingInverted(IEnumerable<string> columnNames) : base(columnNames)
			{
			}

			public override bool AscendingFor(string direction) => !base.AscendingFor(direction);
		}

		private readonly IDictionary<string, ISorting> _sortingByOrderBy = new Dictionary<string, ISorting>()
		{
			{"Name", new Sorting(new[] {"FirstName", "LastName"})},
			{"SiteAndTeamName", new Sorting(new[] {"SiteName", "TeamName"})},
			{"State", new Sorting(new[] {"StateName"})},
			{"Rule", new Sorting(new[] {"RuleName"})},
			{"TimeInAlarm", new SortingInverted(new[] {"AlarmStartTime"})},
			{"TimeInState", new SortingInverted(new[] {"StateStartTime"})},
		};
	}
}