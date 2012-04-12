using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common
{
	public class RunController
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

		public void StartEtlJobRunLock()
		{
			throw new System.NotImplementedException();
		}
	}
}