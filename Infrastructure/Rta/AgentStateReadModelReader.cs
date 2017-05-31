using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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

		public IEnumerable<AgentStateReadModel> ReadFor(AgentStateFilter filter)
		{
			return ReadFor(filter.SiteIds, filter.TeamIds, filter.SkillIds);
		}

		
		public IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			var queryBuilder =
				new AgentStateReadModelQueryBuilder(_now)
					.WithSelection(siteIds, teamIds, skillIds);
			return load(queryBuilder);
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmFor(AgentStateFilter filter)
		{
			return ReadInAlarmFor(filter.SiteIds, filter.TeamIds, filter.SkillIds);
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			var queryBuilder =
				new AgentStateReadModelQueryBuilder(_now)
					.WithSelection(siteIds, teamIds, skillIds)
					.InAlarm();
			return load(queryBuilder);
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingStatesFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStates)
		{
			var queryBuilder =
				new AgentStateReadModelQueryBuilder(_now)
					.WithSelection(siteIds, teamIds, skillIds)
					.InAlarm()
					.Exclude(excludedStates);
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

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingStatesFor(AgentStateFilter filter)
		{
			return ReadInAlarmExcludingStatesFor(filter.SiteIds, filter.TeamIds, filter.SkillIds, filter.ExcludedStates);
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