using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCode.Converters
{
	public interface ICustomSorter : IComparer
	{
		ListSortDirection SortDirection { get; set; }
	}

	public class CustomSorterBlankAlwaysLast : ICustomSorter
	{
		public string PropertyPath { get; set; }
		public ListSortDirection SortDirection { get; set; }
		private readonly PropertyReflector _reflector = new PropertyReflector();

		public int Compare(object x, object y)
		{
			var xStr = getValueAsString(x);
			var yStr = getValueAsString(y);

			if (string.IsNullOrEmpty(xStr) && string.IsNullOrEmpty(yStr))
			{
				return 0;
			}

			if (string.IsNullOrEmpty(xStr))
			{
				return 1;
			}

			if (string.IsNullOrEmpty(yStr))
			{
				return -1;
			}

			if (SortDirection == ListSortDirection.Descending)
			{
				return String.Compare(yStr, xStr, StringComparison.Ordinal);	
			}
			return String.Compare(xStr, yStr, StringComparison.Ordinal);
		}

		private string getValueAsString(object target)
		{
			var val = _reflector.GetValue(target, PropertyPath);
		    if (val is  DateTime && (DateTime)val == DateTime.MinValue)
		    {
		        val = System.Threading.Thread.CurrentThread.CurrentUICulture.Calendar.MinSupportedDateTime;
		    }
            
			return val != null ? val.ToString() : string.Empty;
		}
	}

	

}