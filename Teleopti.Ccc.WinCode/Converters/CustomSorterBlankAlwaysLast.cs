using System.ComponentModel;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCode.Converters
{
	public class CustomSorterBlankAlwaysLast : ICustomSorter
	{
		public string PropertyPath { get; set; }
		public ListSortDirection SortDirection { get; set; }
		private readonly PropertyReflector _reflector = new PropertyReflector();

		public int Compare(object x, object y)
		{
			var xVal = _reflector.GetValue(x,PropertyPath).ToString();
			var yVal = _reflector.GetValue(y, PropertyPath).ToString();

			if (string.IsNullOrEmpty(xVal) && string.IsNullOrEmpty(yVal))
			{
				return 0;
			}

			if (string.IsNullOrEmpty(xVal))
			{
				return 1;
			}

			if (string.IsNullOrEmpty(yVal))
			{
				return -1;
			}

			if (SortDirection == ListSortDirection.Descending)
			{
				return System.String.Compare(yVal, xVal, System.StringComparison.Ordinal);	
			}
			return System.String.Compare(xVal, yVal, System.StringComparison.Ordinal);
		}
	}
}