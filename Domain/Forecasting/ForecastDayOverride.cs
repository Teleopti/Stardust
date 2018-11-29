using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IForecastDayOverride : IAggregateRoot
	{
		DateOnly Date { get; }
		IWorkload Workload { get; }
		IScenario Scenario { get; }
		double OriginalTasks { get; set; }
		TimeSpan OriginalAverageTaskTime { get; set; }
		TimeSpan OriginalAverageAfterTaskTime { get; set; }
		double? OverriddenTasks { get; set; }
		TimeSpan? OverriddenAverageTaskTime { get; set; }
		TimeSpan? OverriddenAverageAfterTaskTime { get; set; }
	}

	public class ForecastDayOverride : VersionedAggregateRoot, IForecastDayOverride
	{
		private readonly DateOnly _date;
		private readonly IWorkload _workload;
		private readonly IScenario _scenario;
		private double _originalTasks;
		private TimeSpan _originalAverageTaskTime;
		private TimeSpan _originalAverageAfterTaskTime;
		private double? _overridedTasks;
		private TimeSpan? _overridedAverageTaskTime;
		private TimeSpan? _overridedAverageAfterTaskTime;

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

		public virtual double OriginalTasks
		{
			get => _originalTasks;
			set => _originalTasks = value;
		}

		public virtual TimeSpan OriginalAverageTaskTime
		{
			get => _originalAverageTaskTime;
			set => _originalAverageTaskTime = value;
		}

		public virtual TimeSpan OriginalAverageAfterTaskTime
		{
			get => _originalAverageAfterTaskTime;
			set => _originalAverageAfterTaskTime = value;
		}

		public virtual double? OverriddenTasks
		{
			get => _overridedTasks;
			set => _overridedTasks = value;
		}

		public virtual TimeSpan? OverriddenAverageTaskTime
		{
			get => _overridedAverageTaskTime;
			set => _overridedAverageTaskTime = value;
		}

		public virtual TimeSpan? OverriddenAverageAfterTaskTime
		{
			get => _overridedAverageAfterTaskTime;
			set => _overridedAverageAfterTaskTime = value;
		}
	}
}