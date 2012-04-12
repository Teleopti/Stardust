using System;
using System.Configuration;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.Common
{
	public class RunController:IDisposable
	{
		private IEtlJobLock _etlJobLock;
		private readonly IRunControllerRepository _repository;

		public RunController(IRunControllerRepository repository)
		{
			_repository = repository;
		}

		public bool CanIRunAJob(out IEtlRunningInformation etlRunningInformation)
		{
			return !_repository.IsAnotherEtlRunningAJob(out etlRunningInformation); 
		}

		public void StartEtlJobRunLock(string jobName, bool isStartByService, IEtlJobLock etlJobLock)
		{
			_etlJobLock = etlJobLock;
			_etlJobLock.CreateLock(jobName,isStartByService);
		}

		public void Dispose()
		{
			_etlJobLock.Dispose();
		}
	}
}