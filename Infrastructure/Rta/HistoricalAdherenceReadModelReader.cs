using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalAdherenceReadModelReader : IHistoricalAdherenceReadModelReader
	{

		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public HistoricalAdherenceReadModelReader(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<HistoricalAdherence> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(@"
SELECT *
FROM [ReadModel].[HistoricalAdherence]
WHERE PersonId = :PersonId
AND [Timestamp] BETWEEN :StartTime AND :EndTime
ORDER BY [Timestamp] ASC")
				.SetParameter("PersonId", personId)
				.SetParameter("StartTime", startTime)
				.SetParameter("EndTime", endTime)
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.List<HistoricalAdherence>();
		}

		public HistoricalAdherence ReadLastBefore(Guid personId, DateTime timestamp)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(@"
SELECT TOP 1 *
FROM [ReadModel].[HistoricalAdherence]
WHERE PersonId = :PersonId
AND [Timestamp] < :Timestamp
ORDER BY [Timestamp] DESC")
				.SetParameter("PersonId", personId)
				.SetParameter("Timestamp", timestamp)
				
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.UniqueResult<HistoricalAdherence>();
		}

		internal class internalModel : HistoricalAdherence
		{
			public new DateTime Timestamp
			{
				set { base.Timestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
			}
		}
	}
}