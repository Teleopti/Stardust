using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.Common
{
	public class RunController : IRunController
	{
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
			etlJobLock.CreateLock(jobName, isStartByService);
		}
	}
}