using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ICalculateAdherenceDetails
	{
		IEnumerable<AdherenceDetailsPercentageModel> ForDetails(Guid personId);
	}
	public class CalculateAdherenceDetails : ICalculateAdherenceDetails
	{
		private readonly INow _now;
		private readonly IAdherenceDetailsReadModelPersister _readModelPersister;
		private readonly CalculateAdherencePercent _calculateAdherencePercent;

		public CalculateAdherenceDetails(INow now, IAdherenceDetailsReadModelPersister readModelPersister)
		{
			_now = now;
			_readModelPersister = readModelPersister;
			_calculateAdherencePercent = new CalculateAdherencePercent(_now);
		}

		public IEnumerable<AdherenceDetailsPercentageModel> ForDetails(Guid personId)
		{
			var readModels = _readModelPersister.Get(personId, new DateOnly(_now.UtcDateTime()));
			var result = new List<AdherenceDetailsPercentageModel>();
			readModels.ForEach(m =>
			{
				if (m == null || !isValid(m))
					return;
				result.Add(new AdherenceDetailsPercentageModel
				{
					Name = m.Name,
					StartTime = m.StartTime.GetValueOrDefault(),
					ActualStartTime = m.ActualStartTime.GetValueOrDefault(),
					AdherencePercent = (int)_calculateAdherencePercent.ForActivity(m).ValueAsPercent()
				});
			});
			return result;
		}


		private static bool isValid(AdherenceDetailsReadModel readModel)
		{
			return !(readModel.TimeInAdherence == TimeSpan.Zero && readModel.TimeOutOfAdherence == TimeSpan.Zero);
		}
	}
}