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
		private readonly IReadActualAgentStates _databaseReader;

		public AdherenceAggregatorInitializor(IAdherenceAggregator aggregator, IReadActualAgentStates databaseReader)
		{
			_aggregator = aggregator;
			_databaseReader = databaseReader;
		}

		public void Initialize()
		{
			foreach (var actualAgentState in _databaseReader.GetActualAgentStates())
				_aggregator.Initialize(actualAgentState);
		}
	}
}