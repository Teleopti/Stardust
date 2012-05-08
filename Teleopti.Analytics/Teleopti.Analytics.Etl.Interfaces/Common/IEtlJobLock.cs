using System;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IEtlJobLock:IDisposable
	{
		void CreateLock(string jobName, bool isStartByService);
	}
}