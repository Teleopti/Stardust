using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public interface IAdherenceAggregatorInitializor
	{
		void Initialize();
	}

	public class AdherenceAggregatorInitializor : IAdherenceAggregatorInitializor
	{
		private readonly IActualAgentStateHasBeenSent _aggregator;
		private readonly IPersonOrganizationProvider _personOrganizationProvider;
		private readonly ILoadActualAgentState _databaseReader;

		public AdherenceAggregatorInitializor(IActualAgentStateHasBeenSent aggregator, IPersonOrganizationProvider personOrganizationProvider, ILoadActualAgentState databaseReader)
		{
			_aggregator = aggregator;
			_personOrganizationProvider = personOrganizationProvider;
			_databaseReader = databaseReader;
		}

		public void Initialize()
		{
			var states = from personOrganizationData in _personOrganizationProvider.LoadAll().Values
				let state = _databaseReader.LoadOldState(personOrganizationData.PersonId)
				where state != null
				select state;
			foreach (var actualAgentState in states)
				_aggregator.Invoke(actualAgentState);
		}
	}
}