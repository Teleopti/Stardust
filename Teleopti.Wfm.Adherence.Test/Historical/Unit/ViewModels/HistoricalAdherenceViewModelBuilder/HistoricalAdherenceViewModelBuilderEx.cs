using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	public static class HistoricalAdherenceViewModelBuilderEx
	{
		public static HistoricalAdherenceViewModel Build(
			this Adherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder target,
			Guid personId) =>
			target.Build(personId, new DateOnly(ServiceLocatorForEntity.Now.UtcDateTime().Date));
	}
}