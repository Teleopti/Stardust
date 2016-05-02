using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IBlockFinder
	{
		IBlockInfo Find(IEnumerable<IScheduleMatrixPro> matrixes, DateOnly blockOnDate, bool singleAgentTeam);
	}
}