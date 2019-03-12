using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class DayProjectionPage
	{
		public DayProjectionPage(int totalDays, int totalPage, List<PersonDayProjectionChanged> pages) : this(totalDays, totalPage)
		{
			Projections = pages;
		}
		public DayProjectionPage(int totalDays, int totalPages)
		{
			Projections = new List<PersonDayProjectionChanged>();
			TotalSchedules = totalDays;
			TotalPages = totalPages;
		}
		public List<PersonDayProjectionChanged> Projections { get; set; }
		public int TotalSchedules { get; set; }
		public int TotalPages { get; set; }
		public int DaysInPage => Projections.Sum(x => x.DaysInRange);
	}
}
