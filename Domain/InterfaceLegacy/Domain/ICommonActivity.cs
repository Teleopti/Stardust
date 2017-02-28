using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ICommonActivity
	{
		IActivity Activity { get; set; }
		IList<DateTimePeriod> Periods { get; set; } 
	}
}