using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class RequestsShiftTradeScheduleFilterViewModel
	{
		public IEnumerable<string> DayOffShortNames { get; set; }
		public IEnumerable<string> HourTexts { get; set; }
		public string EmptyDayText { get; set; }
		public IDictionary<string, string> StartTimeSortOrders { get; set; }
		public IDictionary<string, string> EndTimeSortOrders { get; set; } 
	}
}
