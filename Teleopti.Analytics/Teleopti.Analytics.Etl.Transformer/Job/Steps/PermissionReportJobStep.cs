using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class PermissionReportJobStep : JobStepBase
    {
        public PermissionReportJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "permission_report";
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			//ToDo: get this in some objects instead
			bool isFirstBusinessUnit;
			if (jobResultCollection.Count == 0)
			{
				isFirstBusinessUnit = true;
			}
			else
			{
				isFirstBusinessUnit = false;
			}

			return _jobParameters.Helper.Repository.FillPermissionDataMart(RaptorTransformerHelper.CurrentBusinessUnit, isFirstBusinessUnit, isLastBusinessUnit);
        }
    }






}