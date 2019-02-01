using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Wfm.Adherence.Historical
{
	public class AdjustedPeriodsViewModelBuilder
	{
		public AdjustedPeriodViewModel[] Build()
		{
			return new[] {new AdjustedPeriodViewModel()
			{
				StartTime = "2019-01-28 12:00", 
				EndTime = "2019-01-28 14:00"
			}};
		}
	}
}