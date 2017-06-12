using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelLegacyReader : IAgentStateReadModelLegacyReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public AgentStateReadModelLegacyReader(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons)
		{
			return Read(
				persons
					.Select(person => person.Id.GetValueOrDefault())
					.ToList());
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

		public IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE TeamId IN (:teamIds)")
				.SetParameterList("teamIds", teamIds)
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
