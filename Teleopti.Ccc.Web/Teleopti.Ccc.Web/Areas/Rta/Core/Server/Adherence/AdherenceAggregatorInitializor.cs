using System.Linq;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public interface IAdherenceAggregatorInitializor
	{
		void Initialize();
	}

	public class AdherenceAggregatorInitializor : IAdherenceAggregatorInitializor
	{
		private readonly IAdherenceAggregator _aggregator;
		private readonly IPersonOrganizationProvider _personOrganizationProvider;
		private readonly IReadActualAgentStates _databaseReader;

		public AdherenceAggregatorInitializor(IAdherenceAggregator aggregator, IPersonOrganizationProvider personOrganizationProvider, IReadActualAgentStates databaseReader)
		{
			_aggregator = aggregator;
			_personOrganizationProvider = personOrganizationProvider;
			_databaseReader = databaseReader;
		}

		public void Initialize()
		{
			foreach (var actualAgentState in _databaseReader.GetActualAgentStates())
				_aggregator.Initialize(actualAgentState);
		}
	}
}