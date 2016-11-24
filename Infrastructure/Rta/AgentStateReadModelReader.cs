using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			var queryBuilder = new AgentStateReadModelQueryBuilder(_now);
			if (siteIds != null)
				queryBuilder.InSites(siteIds);
			if (teamIds != null)
				queryBuilder.InTeams(teamIds);
			if (skillIds != null)
				queryBuilder.WithSkills(skillIds);

			var builder = queryBuilder.Build();
			var sqlQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery(builder.Query);
			builder.ParameterFuncs.ForEach(f => f(sqlQuery));
			return sqlQuery
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmsFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			var queryBuilder = new AgentStateReadModelQueryBuilder(_now);
			queryBuilder.InAlarm();
			if (siteIds != null)
				queryBuilder.InSites(siteIds);
			if (teamIds != null)
				queryBuilder.InTeams(teamIds);
			if (skillIds != null)
				queryBuilder.WithSkills(skillIds);

			var builder = queryBuilder.Build();
			var sqlQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery(builder.Query);
			builder.ParameterFuncs.ForEach(f => f(sqlQuery));
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