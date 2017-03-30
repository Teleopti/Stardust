using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class BlockInfoFactory
	{
		public BlockInfo Execute(bool singleAgentTeam, 
			IEnumerable<IScheduleMatrixPro> allMatrixes,
			Func<IScheduleMatrixPro, DateOnlyPeriod?> periodForOneMatrix)
		{
			//TEMP
			if (!singleAgentTeam)
			{
				return new BlockInfo(new DateOnlyPeriod(2015, 1, 1, 2018, 1, 1));
			}
			//

			var roleModelMatrix = allMatrixes.First(); //should be changed
			var period = periodForOneMatrix(roleModelMatrix);
			return period.HasValue ? new BlockInfo(period.Value) : null;
		}
	}
}