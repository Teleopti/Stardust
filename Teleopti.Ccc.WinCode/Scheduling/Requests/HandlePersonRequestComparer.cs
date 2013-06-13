using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
	public abstract class SortComparer : IComparer
	{
		public IList<SortDescription> SortDescriptions { get; set; }
		public abstract int Compare(object x, object y);
	}

	public class HandlePersonRequestComparer : SortComparer
	{
		public HandlePersonRequestComparer()
		{
			SortDescriptions = new List<SortDescription>();
		}

		public override int Compare(object x, object y)
		{
			var result = 0;
			foreach (var sort in SortDescriptions)
			{
				switch (sort.PropertyName)
				{
					case "RequestedDate":
						result = compareRequestedDate(sort, x, y);
						break;
					case "LastUpdatedDisplay":
						result = compareLastUpdatedDisplay(sort, x, y);
						break;
					default:
						var xValue = x.GetType().GetProperty(sort.PropertyName).GetValue(x, null);
						var yValue = y.GetType().GetProperty(sort.PropertyName).GetValue(y, null);

						var xString = xValue as string;
						if (xString != null)
							result = compareString(sort, xString, yValue);

						else if (xValue is int)
							result = compareInt(sort, xValue, yValue);

						else if (xValue is bool)
							result = compareBool(sort, xValue, yValue);
						break;
				}
				if (result != 0) break;
			}
			return result;
		}

		private static int compareRequestedDate(SortDescription sort, object x, object y)
		{
			var xPersonRequestPeriod = ((PersonRequestViewModel) x).PersonRequest.Request.Period;
			var yPersonReuqestPeriod = ((PersonRequestViewModel) y).PersonRequest.Request.Period;

			if (xPersonRequestPeriod.StartDateTime.Date
			                        .Equals(yPersonReuqestPeriod.StartDateTime.Date))
				return sort.Direction == ListSortDirection.Ascending
					       ? xPersonRequestPeriod.EndDateTime.CompareTo(
						       yPersonReuqestPeriod.EndDateTime)
					       : yPersonReuqestPeriod.EndDateTime.CompareTo(
						       xPersonRequestPeriod.EndDateTime);

			return sort.Direction == ListSortDirection.Ascending
				       ? xPersonRequestPeriod.CompareTo(
					       yPersonReuqestPeriod)
				       : yPersonReuqestPeriod.CompareTo(
					       xPersonRequestPeriod);
		}

		private static int compareLastUpdatedDisplay(SortDescription sort, object x, object y)
		{
			var xLastUpdated = ((PersonRequestViewModel) x).LastUpdated;
			var yLastUpdated = ((PersonRequestViewModel) y).LastUpdated;

			return sort.Direction == ListSortDirection.Ascending
				       ? xLastUpdated.CompareTo(yLastUpdated)
				       : yLastUpdated.CompareTo(xLastUpdated);
		}

		private static int compareString(SortDescription sort, string xString, object yValue)
		{
			return sort.Direction == ListSortDirection.Ascending
				       ? String.Compare(xString, (string) yValue, StringComparison.CurrentCulture)
				       : String.Compare((string) yValue, xString, StringComparison.CurrentCulture);
		}

		private static int compareInt(SortDescription sort, object xValue, object yValue)
		{
			return sort.Direction == ListSortDirection.Ascending
				       ? ((int) xValue).CompareTo((int) yValue)
				       : ((int) yValue).CompareTo((int) xValue);
		}

		private static int compareBool(SortDescription sort, object xValue, object yValue)
		{
			return sort.Direction == ListSortDirection.Ascending
				       ? ((bool) xValue).CompareTo((bool) yValue)
				       : ((bool) yValue).CompareTo((bool) xValue);
		}
	}
}
