using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public interface IPersonPeriodFilter
	{
		IEnumerable<IPersonPeriod> GetFiltered(IEnumerable<IPersonPeriod> personPeriods);
	}
}