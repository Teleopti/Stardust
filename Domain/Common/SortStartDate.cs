using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	//implicitly tested by layersextensionsperiodtest
	public class SortStartDate : IComparer<IPeriodized>
	{
		public int Compare(IPeriodized x, IPeriodized y)
		{
			return x.Period.StartDateTime.CompareTo(y.Period.StartDateTime);
		}
	}
}