using System.Collections.Generic;
using System.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IScheduleDayOffCountTransformer //: IEtlTransformer<IScheduleDay>
	{
		void Transform(IEnumerable<IScheduleDay> rootList, DataTable table, int intervalsPerDay);
	}
}
