using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder
{
	public static class HistoricalAdherenceViewModelBuilderEx
	{
		public static HistoricalAdherenceViewModel Build(
			this Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder target,
			Guid personId) =>
			target.Build(personId, new DateOnly(ServiceLocatorForEntity.Now.UtcDateTime().Date));
	}
}