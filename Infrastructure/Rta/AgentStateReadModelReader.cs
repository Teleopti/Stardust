using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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

		public AgentStateReadModelReader(ICurrentUnitOfWork unitOfWork, INow now)
		{
			_unitOfWork = unitOfWork;
			_now = now;
		}

		public IEnumerable<AgentStateReadModel> ReadFor(AgentStateFilter filter)
		{
			var queryBuilder =
				new AgentStateReadModelQueryBuilder(_now)
					.WithSelection(filter.SiteIds, filter.TeamIds, filter.SkillIds);
			if (filter.InAlarm)
				queryBuilder.InAlarm();
			if (filter.ExcludedStates.EmptyIfNull().Any())
				queryBuilder.Exclude(filter.ExcludedStates);
			return load(queryBuilder);
		}
		
		private IEnumerable<AgentStateReadModel> load(AgentStateReadModelQueryBuilder queryBuilder)
		{
			var builder = queryBuilder.Build();
			var sqlQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery(builder.Query);
			builder.ParameterFuncs
				.ForEach(f => f(sqlQuery));
			return sqlQuery
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();

		}
		public IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds)
		{
			return personIds.Batch(400).SelectMany(personIdBatch => _unitOfWork.Current().Session()
					.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE PersonId IN(:persons)")
					.SetParameterList("persons", personIdBatch)
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>())
				.ToArray();
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