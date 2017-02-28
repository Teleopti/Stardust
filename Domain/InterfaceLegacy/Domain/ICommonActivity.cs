using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ICommonActivity
	{
		IActivity Activity { get; set; }
		IList<DateTimePeriod> Periods { get; set; } 
	}
}