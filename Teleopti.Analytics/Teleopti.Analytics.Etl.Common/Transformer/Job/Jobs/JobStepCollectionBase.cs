using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public abstract class JobStepCollectionBase : List<IJobStep>
	{
		protected void AddWhenAllEnabled(IJobStep step, params Toggles[] toggles)
		{
			if (toggles.All(x => step.JobParameters.ToggleManager.IsEnabled(x)))
			{
				Add(step);
			}
		}
		protected void AddWhenAllDisabled(IJobStep step, params Toggles[] toggles)
		{
			if (toggles.All(x => !step.JobParameters.ToggleManager.IsEnabled(x)))
			{
				Add(step);
			}
		}

		protected void AddWhenAnyEnabled(IJobStep step, params Toggles[] toggles)
		{
			if (toggles.Any(x => step.JobParameters.ToggleManager.IsEnabled(x)))
			{
				Add(step);
			}
		}
		protected void AddWhenAnyDisabled(IJobStep step, params Toggles[] toggles)
		{
			if (toggles.Any(x => !step.JobParameters.ToggleManager.IsEnabled(x)))
			{
				Add(step);
			}
		}
	}
}