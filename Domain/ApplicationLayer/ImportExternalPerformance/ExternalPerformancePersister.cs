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

			var allExistData = _externalDataRepository.LoadAll();

			foreach (var validRecord in result.ValidRecords)
			{
				var score = validRecord.MeasureNumberScore;
				if (validRecord.MeasureType == ExternalPerformanceDataType.Percent) score = Convert.ToInt32(validRecord.MeasurePercentScore.Value * 10000);

				var existData = allExistData.FirstOrDefault(x => x.PersonId == validRecord.PersonId && x.DateFrom == validRecord.DateFrom);
				if (existData == null)
				{
					_externalDataRepository.Add(new ExternalPerformanceData()
					{
						ExternalPerformance = externalPerformances.FirstOrDefault(x => x.ExternalId == validRecord.MeasureId),
						DateFrom = validRecord.DateFrom,
						OriginalPersonId = validRecord.AgentId,
						PersonId = validRecord.PersonId,
						Score = score
					});
				}
				else
				{
					existData.Score = score;
				}
			}
		}
	}
}
