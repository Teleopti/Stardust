using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Linq.ReWriters;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		private readonly ICurrentBusinessUnit _businessUnit;

		public AgentStateReadModelReader(ICurrentUnitOfWork unitOfWork, INow now, ICurrentBusinessUnit businessUnit)
		{
			_unitOfWork = unitOfWork;
			_now = now;
			_businessUnit = businessUnit;
		}

		public IEnumerable<AgentStateReadModel> Read(AgentStateFilter filter)
		{
			var builder =
				new AgentStateReadModelQueryBuilder()
					.WithoutDeleted()
					.WithBusinessUnit(_businessUnit.Current().Id.Value)
					.WithSelection(filter.SiteIds, filter.TeamIds, filter.SkillIds, _now)
					.WithTextFilter(filter.TextFilter);
			if (filter.InAlarm)
				builder.InAlarm(_now);
			if (filter.ExcludedStates.EmptyIfNull().Any())
				builder.Exclude(filter.ExcludedStates);
			if (!filter.OrderBy.IsNullOrEmpty())
			{
				var translatedFilter = AgentStateReadModelReader.translatedFilter(filter);
				builder.WithSorting(_now, translatedFilter.OrderBy, translatedFilter.Direction);
			}

			return load(builder);
		}

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds)
		{
			return personIds.Batch(400).SelectMany(personIdsInBatch =>
					load(
						new AgentStateReadModelQueryBuilder()
							.WithoutDeleted()
							.WithBusinessUnit(_businessUnit.Current().Id.Value)
							.WithMax(400)
							.WithPersons(personIdsInBatch)
					))
				.ToArray();
		}

		private IEnumerable<AgentStateReadModel> load(AgentStateReadModelQueryBuilder queryBuilder)
		{
			var result = queryBuilder.Build();
			var sqlQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery(result.Query);
			result.ParameterFuncs
				.ForEach(f => f(sqlQuery));
			return sqlQuery
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		private static SortingFilter translatedFilter(AgentStateFilter filter)
		{
			var sortingFilter = new SortingFilter {Direction = filter.Direction};
			switch (filter.OrderBy)
			{
				case "TimeInAlarm":
					sortingFilter.OrderBy = new [] {"AlarmStartTime"};
					sortingFilter.Direction = switchDirection(sortingFilter);
					break;
				case "TimeInState":
					sortingFilter.OrderBy = new[] {"StateStartTime"};
					sortingFilter.Direction = switchDirection(sortingFilter);
					break;
				case "TimeOutOfAdherence":
					sortingFilter.OrderBy = new[] {"OutOfAdherences"};
					sortingFilter.Direction = switchDirection(sortingFilter);
					break;
				case "Rule":
					sortingFilter.OrderBy = new[] {"RuleName"};
					break;
				case "State":
					sortingFilter.OrderBy = new[] {"StateName"};
					break;
				case "SiteAndTeamName":
					sortingFilter.OrderBy = new[] {"SiteName", "TeamName"};
					break;
				case "Name":
					sortingFilter.OrderBy =  new[]{"FirstName", "LastName"};
					break;
			}
			return sortingFilter;
		}

		private static string switchDirection(SortingFilter filter)
		{
			return filter.Direction == "asc" ? "desc" : "asc";
		}

		internal class SortingFilter
		{
			public IEnumerable<string> OrderBy { get; set; }
			public string Direction { get; set; }
		}
		
		private class internalModel : AgentStateReadModel
		{
			public new string Shift
			{
				set { base.Shift = value != null ? JsonConvert.DeserializeObject<AgentStateActivityReadModel[]>(value) : null; }
			}

			public new string OutOfAdherences
			{
				set { base.OutOfAdherences = value != null ? JsonConvert.DeserializeObject<AgentStateOutOfAdherenceReadModel[]>(value) : null; }
			}
		}
	}

}