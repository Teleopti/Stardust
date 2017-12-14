using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	public static class HistoricalAdherenceViewModelBuilderEx
	{
		public static HistoricalAdherenceViewModel Build(
			this Domain.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder target,
			Guid personId) =>
			target.Build(personId, new DateOnly(ServiceLocatorForEntity.Now.UtcDateTime().Date));
	}
}