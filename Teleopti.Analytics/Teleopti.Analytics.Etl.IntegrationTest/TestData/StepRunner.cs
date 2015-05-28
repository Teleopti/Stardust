using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public static class StepRunner
	{
		public static List<IJobResult> RunBasicStepsBeforeSchedule(JobParameters jobParameters)
		{
			var result = new List<IJobResult>();

			JobStepBase step = new StageBusinessUnitJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimBusinessUnitJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new DimSiteJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// we must run this to get the not defined skillset
			step = new DimSkillSetJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			//organisation
			step = new StagePersonJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimScorecardJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimSiteJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimTeamJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimPersonJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new StageScenarioJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimScenarioJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new StageActivityJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimActivityJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new StageAbsenceJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimAbsenceJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new StageScheduleDayOffCountJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new StageShiftCategoryJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimShiftCategoryJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new StageOvertimeJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimOvertimeJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			return result;
		}

		private static List<IJobResult> RunJob(string jobName, IEnumerable<IJobStep> jobStepCollection)
		{
			var jobList = new List<IJobResult>();
			var Result = new JobResult(TestState.BusinessUnit, jobList)
			{
				Name = jobName,
				Status = string.Format("Running ({0})", TestState.BusinessUnit.Name),
				Success = true
			}; // Inject list of BUs

			foreach (var step in jobStepCollection)
			{
				var jobStepResult = step.Run(new List<IJobStep>(), TestState.BusinessUnit, jobList, true);
				Result.JobStepResultCollection.Add(jobStepResult);

				//Stop the Job if any JobStep goes wrong !!
				if (jobStepResult.JobStepException != null)
				{
					Result.Success = false;
					break;
				}
			}
			jobList.Add(Result);
			return jobList;
		}

		public static List<IJobResult> RunNightly(JobParameters jobParameters)
		{
			var result = new List<IJobResult>();
			var nightly = new NightlyJobCollection(jobParameters);
			foreach (var step in nightly)
			{
				step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			}

			return result;
		}

		public static List<IJobResult> RunIntraday(JobParameters jobParameters)
		{
			var result = new List<IJobResult>();
			var intra = new IntradayJobCollection(jobParameters);
			foreach (var step in intra)
			{
				step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			}

			return result;
		}
	}
}