using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
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

		public HistoricalAdherenceReadModel Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			var result = _unitOfWork.Current()
				.CreateSqlQuery(@"
SELECT *
FROM [ReadModel].[HistoricalAdherence]
WHERE PersonId = :PersonId
AND [Timestamp] BETWEEN :StartTime AND :EndTime
ORDER BY [Timestamp] ASC")
				.SetParameter("PersonId", personId)
				.SetParameter("StartTime", startTime)
				.SetParameter("EndTime", endTime)
				.SetResultTransformer(Transformers.AliasToBean<HistoricalAdherenceInternalModel>())
				.List<HistoricalAdherenceInternalModel>();

			return BuildReadModel(result, personId);
		}

		public static HistoricalAdherenceReadModel BuildReadModel(IEnumerable<HistoricalAdherenceInternalModel> data, Guid personId)
		{
			var seed = new HistoricalAdherenceReadModel
			{
				PersonId = personId,
				OutOfAdherences = Enumerable.Empty<HistoricalOutOfAdherenceReadModel>()
			};
			return data.Aggregate(seed, (x, im) =>
			{
				if (im.Adherence == HistoricalAdherenceInternalAdherence.Out)
				{
					if (x.OutOfAdherences.IsEmpty(y => !y.EndTime.HasValue))
						x.OutOfAdherences = x.OutOfAdherences
							.Append(new HistoricalOutOfAdherenceReadModel {StartTime = im.Timestamp})
							.ToArray();
				}
				else
				{
					var existing = x.OutOfAdherences.FirstOrDefault(y => !y.EndTime.HasValue);
					if (existing != null)
						existing.EndTime = im.Timestamp;
				}

				return x;
			});
		}
	}

	public class HistoricalAdherenceInternalModel
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public HistoricalAdherenceInternalAdherence Adherence { get; set; }
	}

	public enum HistoricalAdherenceInternalAdherence
	{
		In,
		Neutral,
		Out
	}
}