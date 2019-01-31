using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public class Selection
	{
		public string Query { get; set; }
		public IEnumerable<Func<ISQLQuery, IQuery>> ParameterFuncs { get; set; }
	}

	public class AgentStateReadModelQueryBuilderConfiguration
	{
		private readonly Lazy<IDictionary<string, Sorting>> _sortingByOrderBy;

		public AgentStateReadModelQueryBuilderConfiguration(IGlobalSettingDataRepository settings)
		{
			_sortingByOrderBy = new Lazy<IDictionary<string, Sorting>>(() =>
			{
				var format = settings
						.FindValueByKey(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting())
						.BuildNamesFor("FirstName", "LastName", "EmploymentNumber")
					;

				return new Dictionary<string, Sorting>
				{
					{"Name", new Sorting {Columns = format}},
					{"SiteAndTeamName", new Sorting {Columns = new[] {"SiteName", "TeamName"}}},
					{"State", new Sorting {Columns = new[] {"StateName"}}},
					{"Rule", new Sorting {Columns = new[] {"RuleName"}}},
					{"TimeInAlarm", new SortingInverted {Columns = new[] {"AlarmStartTime"}}},
					{"TimeInState", new SortingInverted {Columns = new[] {"StateStartTime"}}},
					{"TimeOutOfAdherence", new SortingInverted {Columns = new[] {"ISNULL(OutOfAdherenceStartTime, '9999-12-31 23:59:59')"}}},
				};
			});
		}

		public Sorting SortingFor(string orderBy) => _sortingByOrderBy.Value[orderBy];

		public class Sorting
		{
			public IEnumerable<string> Columns { get; set; }
			public virtual bool AscendingFor(string direction) => direction != "desc";
		}

		public class SortingInverted : Sorting
		{
			public override bool AscendingFor(string direction) => !base.AscendingFor(direction);
		}
	}

	public class AgentStateReadModelQueryBuilder
	{
		private readonly AgentStateReadModelQueryBuilderConfiguration _configuration;
		private readonly IList<string> _froms = new List<string>();
		private readonly IList<string> _wheres = new List<string>();
		private readonly IList<string> _orderbys = new List<string>();
		private readonly IList<Func<ISQLQuery, IQuery>> _parameters = new List<Func<ISQLQuery, IQuery>>();
		private int _topCount;

		public AgentStateReadModelQueryBuilder(AgentStateReadModelQueryBuilderConfiguration configuration)
		{
			_configuration = configuration;
			_topCount = 50;
		}

		public AgentStateReadModelQueryBuilder IsActivated()
		{
			_wheres.Add("HasAssociation = 1");
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
					.Select((x, i) =>
					{
						var exclude = x.StartsWith("-");
						return new
						{
							word = exclude ? x.Substring(1) : x,
							modifier = exclude ? "NOT" : "",
							index = i
						};
					})
					.Where(x => !string.IsNullOrWhiteSpace(x.word))
					.ForEach(x =>
					{
						_wheres.Add($"a.TextFilter {x.modifier} LIKE :textFilter{x.index}");
						_parameters.Add(s => s.SetParameter($"textFilter{x.index}", $"%{x.word}%"));
					});
			}

			return this;
		}

		// put skills in its own With method probably ;)
		public AgentStateReadModelQueryBuilder WithSelection(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, INow now)
		{
			var teamSiteWheres = new List<string>();
			if (!siteIds.IsNullOrEmpty())
			{
				teamSiteWheres.Add("a.SiteId IN (:siteIds)");
				_parameters.Add(s => s.SetParameterList("siteIds", siteIds));
			}

			if (!teamIds.IsNullOrEmpty())
			{
				teamSiteWheres.Add("a.TeamId IN (:teamIds)");
				_parameters.Add(s => s.SetParameterList("teamIds", teamIds));
			}

			if (teamSiteWheres.Any())
				_wheres.Add($"({string.Join(" OR ", teamSiteWheres)})");

			if (!skillIds.IsNullOrEmpty())
			{
				_wheres.Add(@"
					EXISTS(
						SELECT * FROM ReadModel.GroupingReadOnly AS g 
						WHERE 
							g.PageId = :skillGroupingPageId AND :today BETWEEN g.StartDate and g.EndDate AND
							a.PersonId = g.PersonId AND
							g.GroupId IN (:SkillIds)
					)
					");
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
				SELECT TOP {_topCount} 
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
					[OutOfAdherenceStartTime],
					[StateGroupId], 
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

		public AgentStateReadModelQueryBuilderConfiguration.Sorting SortingFor(string orderBy) => _configuration.SortingFor(orderBy);

		public AgentStateReadModelQueryBuilder WithSorting(AgentStateReadModelQueryBuilderConfiguration.Sorting sorting, string direction)
		{
			var ascending = sorting.AscendingFor(direction);
			sorting?.Columns.ForEach(c => _orderbys.Add($"{c} {(ascending ? "" : "DESC")}"));
			return this;
		}
	}
}