using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public interface ICreateIslands
	{
		IEnumerable<IIsland> Create(DateOnlyPeriod period);
	}
}