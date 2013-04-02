using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public interface INeedToRunChecker
	{
		bool NeedToRun(DateTimePeriod onPeriod, IRaptorRepository raptorRepository, IBusinessUnit currentBusinessUnit, string stepName);
	}

	public class DefaultNeedToRunChecker: INeedToRunChecker
	{
		public bool NeedToRun(DateTimePeriod onPeriod, IRaptorRepository raptorRepository, IBusinessUnit currentBusinessUnit, string stepName)
		{
			return true;
		}
	}

	public class IntradyScheduleStepNeedToRunChecker : INeedToRunChecker
	{
		public bool NeedToRun(DateTimePeriod onPeriod, IRaptorRepository raptorRepository, IBusinessUnit currentBusinessUnit, string stepName)
		{
			return raptorRepository.DataOnStepHasChanged(onPeriod,currentBusinessUnit,stepName);
		}
	}
}