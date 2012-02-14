using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Matrix;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StagePermissionJobStep : JobStepBase
    {
        public StagePermissionJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_permission";
            PermissionReportInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            _jobParameters.Helper.Repository.TruncatePermissionReportTable();
            IList<MatrixPermissionHolder> reportPermissions = _jobParameters.Helper.Repository.LoadReportPermissions();
            int affectedRows = 0;
            PermissionReportTransformer transformer = new PermissionReportTransformer();

            foreach (IList<MatrixPermissionHolder> permissionHolders in reportPermissions.Batch(500))
            {
                BulkInsertDataTable1.Rows.Clear();
                transformer.Transform(permissionHolders, BulkInsertDataTable1);
                affectedRows += _jobParameters.Helper.Repository.PersistPermissionReport(BulkInsertDataTable1);
            }

            return affectedRows;
        }
    }
}