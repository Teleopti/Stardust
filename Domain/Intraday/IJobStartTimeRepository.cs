using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IJobStartTimeRepository
	{
		bool CheckAndUpdate(int thresholdMinutes);
		void Update(Guid buId);
	}
}