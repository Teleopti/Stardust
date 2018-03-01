using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Linq.ReWriters;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta.Service;
using Teleopti.Ccc.Domain.Rta.ViewModels;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly AgentStateReadModelQueryBuilderConfiguration _configuration;

		public AgentStateReadModelReader(ICurrentUnitOfWork unitOfWork, INow now, ICurrentBusinessUnit businessUnit, AgentStateReadModelQueryBuilderConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_now = now;
			_businessUnit = businessUnit;
			_configuration = configuration;
		}

		public IEnumerable<AgentStateReadModel> Read(AgentStateFilter filter)
		{
			var builder =
				new AgentStateReadModelQueryBuilder(_configuration)
					.WithoutDeleted()
					.WithBusinessUnit(_businessUnit.Current().Id.Value)
					.WithSelection(filter.SiteIds, filter.TeamIds, filter.SkillIds, _now)
					.WithTextFilter(filter.TextFilter);
			if (filter.InAlarm)
				builder.InAlarm(_now);
			if (filter.ExcludedStateIds.EmptyIfNull().Any())
				builder.Exclude(filter.ExcludedStateIds);
			if (!filter.OrderBy.IsNullOrEmpty())
				builder.WithSorting(builder.SortingFor(filter.OrderBy), filter.Direction);

			return load(builder);
		}

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds)
		{
			return personIds.Batch(400).SelectMany(personIdsInBatch =>
					load(
						new AgentStateReadModelQueryBuilder(_configuration)
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