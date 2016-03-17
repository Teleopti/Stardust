using Teleopti.Analytics.Etl.Common.Interfaces.Common;

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
	}
}