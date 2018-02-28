using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Rta.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	public static class HistoricalAdherenceViewModelBuilderEx
	{
		public static HistoricalAdherenceViewModel Build(
			this Domain.Rta.ViewModels.HistoricalAdherenceViewModelBuilder target,
			Guid personId) =>
			target.Build(personId, new DateOnly(ServiceLocatorForEntity.Now.UtcDateTime().Date));
	}
}