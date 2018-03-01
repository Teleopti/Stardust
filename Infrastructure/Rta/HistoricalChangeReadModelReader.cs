using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalChangeReadModelReader : IHistoricalChangeReadModelReader
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public HistoricalChangeReadModelReader(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<HistoricalChangeModel> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			var result = _unitOfWork.Current()
				.CreateSqlQuery(@"
SELECT *
FROM [ReadModel].[HistoricalChange]
WHERE PersonId = :PersonId
AND [Timestamp] BETWEEN :StartTime AND :EndTime
ORDER BY [Timestamp] ASC")
				.SetParameter("PersonId", personId)
				.SetParameter("StartTime", startTime)
				.SetParameter("EndTime", endTime)
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.List<internalModel>();

			return result;
		}

		public HistoricalChangeModel ReadLastBefore(Guid personId, DateTime timestamp)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(@"
SELECT TOP 1 *
FROM [ReadModel].[HistoricalChange]
WHERE PersonId = :PersonId
AND [Timestamp] < :Timestamp
ORDER BY [Timestamp] DESC")
				.SetParameter("PersonId", personId)
				.SetParameter("Timestamp", timestamp)
				
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.UniqueResult<HistoricalChangeModel>();
		}

		private class internalModel : HistoricalChangeModel
		{
			public new DateTime? BelongsToDate
			{
				set { base.BelongsToDate = value.HasValue ? new DateOnly(value.Value) : (DateOnly?) null; }
			}

			public new DateTime Timestamp
			{
				set { base.Timestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc); }
			}

			public new int Adherence { set { base.Adherence = (HistoricalChangeAdherence) value; } }
		}
	}
}