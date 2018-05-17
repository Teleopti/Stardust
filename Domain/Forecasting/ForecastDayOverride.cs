using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IForecastDayOverride : IAggregateRoot
	{
		DateOnly Date { get; }
		IWorkload Workload { get; }
		IScenario Scenario { get; }
	}

	public class ForecastDayOverride : VersionedAggregateRoot, IForecastDayOverride
	{
		private readonly DateOnly _date;
		private readonly IWorkload _workload;
		private readonly IScenario _scenario;

		protected ForecastDayOverride()
		{
		}

		public ForecastDayOverride(DateOnly date, IWorkload workload, IScenario scenario) : this()
		{
			_date = date;
			_workload = workload;
			_scenario = scenario;
		}

		public virtual DateOnly Date => _date;

		public virtual IWorkload Workload => _workload;

		public virtual IScenario Scenario => _scenario;
	}
}