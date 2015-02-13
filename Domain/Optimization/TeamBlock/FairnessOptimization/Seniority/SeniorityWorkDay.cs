using System;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public class SeniorityWorkDay : ISeniorityWorkDay
	{
		public DayOfWeek DayOfWeek { get; set; }
		public int Rank { get; set; }
		public string DayOfWeekName { get; private set; }
		
		public SeniorityWorkDay(DayOfWeek dayOfWeek, int rank)
		{
			DayOfWeek = dayOfWeek;
			Rank = rank;

			switch (DayOfWeek)
			{
				case DayOfWeek.Monday: DayOfWeekName = Resources.Monday; break;
				case DayOfWeek.Tuesday: DayOfWeekName = Resources.Tuesday; break;
				case DayOfWeek.Wednesday: DayOfWeekName = Resources.Wednesday; break;
				case DayOfWeek.Thursday: DayOfWeekName = Resources.Thursday; break;
				case DayOfWeek.Friday: DayOfWeekName = Resources.Friday; break;
				case DayOfWeek.Saturday: DayOfWeekName = Resources.Saturday; break;
				case DayOfWeek.Sunday: DayOfWeekName = Resources.Sunday; break;
				default: DayOfWeekName = Resources.NA; break;
			}		
		}
	}
}
