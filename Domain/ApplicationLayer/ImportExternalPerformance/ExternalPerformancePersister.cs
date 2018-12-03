using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
			if (!externalPerformances.Any() || !result.ValidRecords.Any()) return;

			var dates = result.ValidRecords.Select(x => x.DateFrom).ToList();
			
			var existingDataWithinPeriod = _externalDataRepository.FindByPeriod(new DateOnlyPeriod(dates.Min(), dates.Max()));
			foreach (var record in result.ValidRecords)
			{
				var score = record.MeasureNumberScore;
				if (record.MeasureType == ExternalPerformanceDataType.Percent) score = record.MeasurePercentScore.Value;

				var existingData = existingDataWithinPeriod.FirstOrDefault(x =>
					x.PersonId == record.PersonId && x.DateFrom == record.DateFrom &&
					x.ExternalPerformance.ExternalId == record.MeasureId);

				if (existingData == null)
				{
					_externalDataRepository.Add(new ExternalPerformanceData
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
