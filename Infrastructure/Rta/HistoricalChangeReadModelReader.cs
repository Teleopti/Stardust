using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
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

		public IEnumerable<HistoricalChangeReadModel> Read(Guid personId, DateTime startTime, DateTime endTime)
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

		private class internalModel : HistoricalChangeReadModel
		{
			public new DateTime? BelongsToDate { set
			{
				base.BelongsToDate = value.HasValue ? new DateOnly(value.Value) : (DateOnly?) null;
			} }
			public new int Adherence { set { base.Adherence = (HistoricalChangeInternalAdherence) value; } }
		}
	}
}