using System;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public interface IEtlJobLock:IDisposable
	{
		void CreateLock(string jobName, bool isStartByService);
	}
}