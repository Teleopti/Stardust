using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using Newtonsoft.Json;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
				.CreateSqlQuery("SELECT * FROM [ReadModel].[HistoricalAdherence] WHERE PersonId = :PersonId AND [Timestamp] BETWEEN :StartTime AND :EndTime")
				.SetParameter("PersonId", personId)
				.SetParameter("StartTime", startTime)
				.SetParameter("EndTime", endTime)
				.SetResultTransformer(Transformers.AliasToBean(typeof(HIstoricalAdherenceInternalModel)))
				.List<HIstoricalAdherenceInternalModel>();

			return BuildReadModel(result, personId);
		}

		public static HistoricalAdherenceReadModel BuildReadModel(IEnumerable<HIstoricalAdherenceInternalModel> data, Guid personId)
		{
			var seed = new HistoricalAdherenceReadModel
			{
				PersonId = personId
			};
			return data
				  .Aggregate(seed, (x, im) =>
				  {
					  if (im.Adherence == 2)
						  x.OutOfAdherences = x.OutOfAdherences
						  .EmptyIfNull()
						  .Append(new HistoricalOutOfAdherenceReadModel { StartTime = im.Timestamp })
						  .ToArray();
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

	public class HIstoricalAdherenceInternalModel
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public int Adherence { get; set; }
	}
}