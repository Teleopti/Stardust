using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	public abstract class SortComparer : IComparer
	{
		public IList<SortDescription> SortDescriptions { get; set; }
		public abstract int Compare(object x, object y);
	}

	public class HandlePersonRequestComparer : SortComparer
	{
		private struct HandlePersonRequestHolder
		{
			public readonly PersonRequestViewModel X;
			public readonly PersonRequestViewModel Y;

			public HandlePersonRequestHolder(PersonRequestViewModel x, PersonRequestViewModel y)
			{
				X = x;
				Y = y;
			}
		}

		public HandlePersonRequestComparer()
		{
			SortDescriptions = new List<SortDescription>();
		}

		public override int Compare(object x, object y)
		{
			var result = 0;
			foreach (var sort in SortDescriptions)
			{
				var holder = decideObjectOrder(x, y, sort);
				var xModel = holder.X;
				var yModel = holder.Y;

				switch (sort.PropertyName)
				{
					case "RequestedDate":
						result = compareRequestedDate(xModel, yModel);
						break;
					case "LastUpdatedDisplay":
						result = compareLastUpdatedDisplay(xModel, yModel);
						break;
					default:
						var xValue = xModel.GetType().GetProperty(sort.PropertyName).GetValue(xModel, null);
						var yValue = yModel.GetType().GetProperty(sort.PropertyName).GetValue(yModel, null);

						var xString = xValue as string;
						if (xString != null)
							result = compareString(xString, (string) yValue);

						else if (xValue is int)
							result = compareInt((int) xValue, (int) yValue);

						else if (xValue is bool)
							result = compareBool((bool) xValue, (bool) yValue);
						break;
				}
				if (result != 0) break;
			}
			return result;
		}

		private static HandlePersonRequestHolder decideObjectOrder(object x, object y, SortDescription sortDescription)
		{
			return sortDescription.Direction == ListSortDirection.Ascending
				       ? new HandlePersonRequestHolder((PersonRequestViewModel) x, (PersonRequestViewModel) y)
				       : new HandlePersonRequestHolder((PersonRequestViewModel) y, (PersonRequestViewModel) x);
		}

		private static int compareRequestedDate(IPersonRequestViewModel x, IPersonRequestViewModel y)
		{
			var xPeriod = x.PersonRequest.Request.Period;
			var yPeriod = y.PersonRequest.Request.Period;
			
			return xPeriod.StartDateTime.Date.Equals(yPeriod.StartDateTime.Date) 
				? xPeriod.EndDateTime.CompareTo(yPeriod.EndDateTime) 
				: xPeriod.CompareTo(yPeriod);
		}

		private static int compareLastUpdatedDisplay(IPersonRequestViewModel x, IPersonRequestViewModel y)
		{
			return x.LastUpdated.CompareTo(y.LastUpdated);
		}

		private static int compareString(string xString, string yString)
		{
			return String.Compare(xString, yString, StringComparison.CurrentCulture);
		}

		private static int compareInt(int xInt, int yInt)
		{
			return xInt.CompareTo(yInt);
		}

		private static int compareBool(bool xBool, bool yBool)
		{
			return xBool.CompareTo(yBool);
		}
	}
}
