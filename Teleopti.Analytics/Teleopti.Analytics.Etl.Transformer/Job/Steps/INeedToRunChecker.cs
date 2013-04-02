using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
	public interface INeedToRunChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
		bool NeedToRun(DateTimePeriod onPeriod, IRaptorRepository raptorRepository, IBusinessUnit currentBusinessUnit, string stepName);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
	public class DefaultNeedToRunChecker: INeedToRunChecker
	{
		public bool NeedToRun(DateTimePeriod onPeriod, IRaptorRepository raptorRepository, IBusinessUnit currentBusinessUnit, string stepName)
		{
			return true;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
	public class IntradayScheduleStepNeedToRunChecker : INeedToRunChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool NeedToRun(DateTimePeriod onPeriod, IRaptorRepository raptorRepository, IBusinessUnit currentBusinessUnit, string stepName)
		{
			return raptorRepository.DataOnStepHasChanged(onPeriod,currentBusinessUnit,stepName);
		}
	}
}