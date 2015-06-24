using System;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class WorkloadEvaluateViewModel
	{
		public Guid WorkloadId { get; set; }
		public string Name { get; set; }
		public dynamic[] ForecastMethods { get; set; }
		public dynamic[] Days { get; set; }
		public dynamic ForecastMethodRecommended { get; set; }
	}

	public class WorkloadForecastResultViewModel
	{
		public Guid WorkloadId { get; set; }
		public dynamic[] Days { get; set; }
	}

	public class WorkloadQueueStatisticsViewModel
	{
		public Guid WorkloadId { get; set; }
		public dynamic[] QueueStatisticsDays { get; set; }
	}

	public class WorkloadEvaluateDevViewModel
	{
		public Guid WorkloadId { get; set; }
		public string WorkloadName { get; set; }
		public WorkloadEvaluateDevMethodViewModel[] Methods { get; set; }
	}

	public class WorkloadEvaluateDevMethodViewModel
	{
		public dynamic[] Days { get; set; }
		public ForecastMethodType MethodId { get; set; }
		public double AccuracyNumber { get; set; }
		public bool IsSelected { get; set; }
		public DateTime PeriodEvaluateOnStart { get; set; }
		public DateTime PeriodEvaluateOnEnd { get; set; }
		public DateTime PeriodUsedToEvaluateStart { get; set; }
		public DateTime PeriodUsedToEvaluateEnd { get; set; }
	}
}