using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class MaintenanceJobStep : JobStepBase
    {
        public MaintenanceJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "Maintenance";
            IsBusinessUnitIndependent = true;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            // Purge Audit Trail tables in raptor db. No return value here
            //_jobParameters.Helper.Repository.PurgeAuditTrailTables();
            //Run Delayed job
            _jobParameters.Helper.Repository.RunDelayedJob();
            // Run maintenance sp
            return _jobParameters.Helper.Repository.PerformMaintenance();
        }
    }
}