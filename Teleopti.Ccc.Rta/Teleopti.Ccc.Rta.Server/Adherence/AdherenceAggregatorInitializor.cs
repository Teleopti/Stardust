using System.Linq;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AdherenceAggregatorInitializor 
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
			var states = from d in _personOrganizationProvider.LoadAll()
				let state = _databaseReader.LoadOldState(d.PersonId)
				select state;
			foreach (var actualAgentState in states)
				_aggregator.Invoke(actualAgentState);
		}
	}
}