using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class PermissionReportJobStep : JobStepBase
    {
	    private readonly bool _checkIfNeeded;

	    public PermissionReportJobStep(IJobParameters jobParameters, bool checkIfNeeded = false)
            : base(jobParameters)
		{
			_checkIfNeeded = checkIfNeeded;
			Name = "permission_report";
		}


	    protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			//ToDo: get this in some objects instead
			var isFirstBusinessUnit = jobResultCollection != null && jobResultCollection.Count == 0;

			if (isFirstBusinessUnit)
			{
				_jobParameters.Helper.Repository.TruncatePermissionReportNonactive();
			}

			if (_checkIfNeeded)
			{
				if (!_jobParameters.StateHolder.PermissionsMustRun()) return 0;
			}

		    var rows = _jobParameters.Helper.Repository.FillPermissionDataMart(RaptorTransformerHelper.CurrentBusinessUnit, isFirstBusinessUnit, isLastBusinessUnit);
			_jobParameters.StateHolder.UpdateThisTime("Permissions", RaptorTransformerHelper.CurrentBusinessUnit);
		    return rows;
        }
    }






}