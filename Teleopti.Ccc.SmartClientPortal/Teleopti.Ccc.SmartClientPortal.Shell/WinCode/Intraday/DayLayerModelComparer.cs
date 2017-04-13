using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.WinCode.Scheduling.Requests;

namespace Teleopti.Ccc.WinCode.Intraday
{
	public class DayLayerModelComparer : SortComparer
	{
		private struct DayLayerModelHolder
		{
			public readonly DayLayerModel X;
			public readonly DayLayerModel Y;

			public DayLayerModelHolder(DayLayerModel xModel, DayLayerModel yModel)
			{
				X = xModel;
				Y = yModel;
			}
		}

		public DayLayerModelComparer()
		{
			SortDescriptions = new List<SortDescription>();
		}

		public override int Compare(object x, object y)
		{
			var holder = decideObjectOrder(x, y);
			var dayLayerModelX = holder.X;
			var dayLayerModelY = holder.Y;
			var sortProperty = SortDescriptions.FirstOrDefault().PropertyName;

			switch (sortProperty)
			{
				case "Team.Description.Name" :
					return compareString(dayLayerModelX.Team.Description.Name, dayLayerModelY.Team.Description.Name);
				case "CommonNameDescription":
					return compareString(dayLayerModelX.CommonNameDescription, dayLayerModelY.CommonNameDescription);
				case "CurrentStateDescription":
					return compareString(dayLayerModelX.CurrentStateDescription, dayLayerModelY.CurrentStateDescription);
				case "CurrentActivityDescription":
					return compareString(dayLayerModelX.CurrentActivityDescription, dayLayerModelY.CurrentActivityDescription);
				case"NextActivityDescription":
					return compareString(dayLayerModelX.NextActivityDescription, dayLayerModelY.NextActivityDescription);
				case "NextActivityStartDateTime":
					return compareDateTime(dayLayerModelX.NextActivityStartDateTime, dayLayerModelY.NextActivityStartDateTime);
				case "AlarmDescription":
					return compareString(dayLayerModelX.AlarmDescription, dayLayerModelY.AlarmDescription);
				case "EnteredCurrentState":
					return compareDateTime(dayLayerModelX.EnteredCurrentState, dayLayerModelY.EnteredCurrentState);
				case "IsPinned":
					return compareBool(dayLayerModelX.IsPinned, dayLayerModelY.IsPinned);
				case "ScheduleStartDateTime":
					return compareDateTime(dayLayerModelX.ScheduleStartDateTime, dayLayerModelY.ScheduleStartDateTime);
			}
			return 0;
		}

		private DayLayerModelHolder decideObjectOrder(object x, object y)
		{
			return SortDescriptions.FirstOrDefault().Direction == ListSortDirection.Ascending
				       ? new DayLayerModelHolder((DayLayerModel) x, (DayLayerModel) y)
				       : new DayLayerModelHolder((DayLayerModel) y, (DayLayerModel) x);
		}

		private static int compareBool(bool xBool, bool yBool)
		{
			return xBool.CompareTo(yBool);
		}

		private static int compareDateTime(DateTime xDateTime, DateTime yDateTime)
		{
			return (new TimeSpan(xDateTime.Ticks)).TotalSeconds
			                                      .CompareTo((new TimeSpan(yDateTime.Ticks)).TotalSeconds);
		}

		private static int compareString(string xString, string yString)
		{
			if (string.IsNullOrEmpty(xString))
				return string.IsNullOrEmpty(yString) 
					? 0 
					: 1;
			if (string.IsNullOrEmpty(yString))
				return -1;
			
			return String.Compare(xString, yString, StringComparison.CurrentCulture);
		}
	}
}
