using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using Newtonsoft.Json;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalAdherenceReadModelReader : IHistoricalAdherenceReadModelReader
	{

		private readonly ICurrentUnitOfWork _unitOfWork;

		public HistoricalAdherenceReadModelReader(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public HistoricalAdherenceReadModel Read(Guid personId, DateOnly date)
		{
			var selectHistoricalAdherence = @"SELECT * FROM [ReadModel].HistoricalAdherence {0}";
			var query = string.Format(selectHistoricalAdherence, @"WHERE PersonId = :personId AND [Date] = :date");
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameter("personId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalState)))
				.UniqueResult<HistoricalAdherenceReadModel>();

		}

		private class internalState : HistoricalAdherenceReadModel
		{
			public new string OutOfAdherences
			{
				set
				{
					//base.OutOfAdherences = value != null ? JsonConvert.DeserializeObject<IEnumerable<AgentStateOutOfAdherenceReadModel>>(value) : null;
				}
			}

			public new DateTime Date
			{
				set
				{
					base.Date = new DateOnly(value);
				}
			}

		}

	}
}