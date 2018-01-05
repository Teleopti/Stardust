using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ExternalPerformancePersister : IExternalPerformancePersister
	{
		private readonly IExternalPerformanceRepository _externalPerformanceRepository;
		private readonly IExternalPerformanceDataRepository _externalDataRepository;

		public ExternalPerformancePersister(IExternalPerformanceRepository externalPerformanceRepository, IExternalPerformanceDataRepository externalDataRepository)
		{
			_externalPerformanceRepository = externalPerformanceRepository;
			_externalDataRepository = externalDataRepository;
		}

		public void Persist(ExternalPerformanceInfoProcessResult result)
		{
			foreach (var externalPerformance in result.ExternalPerformances)
			{
				_externalPerformanceRepository.Add(externalPerformance);
			}

			var externalPerformances = result.ExternalPerformances;
			externalPerformances.AddRange(_externalPerformanceRepository.FindAllExternalPerformances());
			if (!externalPerformances.Any()) return;

			var allExistingData = _externalDataRepository.LoadAll();

			foreach (var record in result.ValidRecords)
			{
				var score = record.MeasureNumberScore;
				if (record.MeasureType == ExternalPerformanceDataType.Percent) score = Convert.ToInt32(record.MeasurePercentScore.Value * 10000);

				var existingData = allExistingData.FirstOrDefault(x =>
					x.PersonId == record.PersonId && x.DateFrom == record.DateFrom &&
					x.ExternalPerformance.ExternalId == record.MeasureId);

				if (existingData == null)
				{
					_externalDataRepository.Add(new ExternalPerformanceData()
					{
						ExternalPerformance = externalPerformances.FirstOrDefault(x => x.ExternalId == record.MeasureId),
						DateFrom = record.DateFrom,
						OriginalPersonId = record.AgentId,
						PersonId = record.PersonId,
						Score = score
					});
				}
				else
				{
					existingData.Score = score;
				}
			}
		}
	}
}
