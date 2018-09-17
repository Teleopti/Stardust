using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;

namespace Teleopti.Wfm.Adherence.Test.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder
{
	public static class HistoricalAdherenceViewModelBuilderEx
	{
		public static HistoricalAdherenceViewModel Build(
			this Adherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder target,
			Guid personId) =>
			target.Build(personId, new DateOnly(ServiceLocatorForEntity.Now.UtcDateTime().Date));
	}
}