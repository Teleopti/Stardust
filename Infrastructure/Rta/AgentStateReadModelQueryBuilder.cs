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
		private readonly INow _now;

		public AgentStateReadModelQueryBuilder(INow now, ICurrentBusinessUnit businessUnit)
		{
			_now = now;
			_wheres.Add("IsDeleted = 0");
			_wheres.Add("a.BusinessUnitId = :BusinessUnitId");
			_parameters.Add(s => s.SetGuid("BusinessUnitId", businessUnit.Current().Id.Value));
		}

		public AgentStateReadModelQueryBuilder WithSelection(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
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
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Id)
				);
			}
			return this;
		}

		public AgentStateReadModelQueryBuilder InAlarm()
		{
			_wheres.Add("AlarmStartTime <= :now ");
			_orderbys.Add("AlarmStartTime ASC");
			_parameters.Add(s => s.SetParameter("now", _now.UtcDateTime()));
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
			var builder = new StringBuilder("SELECT DISTINCT TOP 50 a.* FROM [ReadModel].AgentState a WITH (NOLOCK) ");

			_froms.ForEach(s => builder.Append(s));

			builder.Append(" WHERE ");
			builder.Append(string.Join(" AND ", _wheres));

			if (_orderbys.Any())
			{
				builder.Append(" ORDER BY ");
				_orderbys.ForEach(s =>
				{
					builder.Append(s);
				});
			}
			
			return new Selection
			{
				Query = builder.ToString(),
				ParameterFuncs = _parameters
			};
		}
	}
}