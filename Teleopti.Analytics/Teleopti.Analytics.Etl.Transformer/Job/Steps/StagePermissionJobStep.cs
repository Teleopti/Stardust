using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Matrix;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class StagePermissionJobStep : JobStepBase
	{
		private readonly bool _checkIfNeeded;

		public StagePermissionJobStep(IJobParameters jobParameters, bool checkIfNeeded = false)
			: base(jobParameters)
		{
			_checkIfNeeded = checkIfNeeded;
			Name = "stg_permission";
			PermissionReportInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			_jobParameters.Helper.Repository.TruncatePermissionReportTable();
			if (_checkIfNeeded)
			{
				var lastTime = _jobParameters.Helper.Repository.LastChangedDate(Result.CurrentBusinessUnit, "Permissions");
				_jobParameters.StateHolder.SetThisTime(lastTime, "Permissions");
				if (!_jobParameters.StateHolder.PermissionsMustRun()) return 0;
			}
			var reportPermissions = _jobParameters.Helper.Repository.LoadReportPermissions();
			var affectedRows = 0;
			var transformer = new PermissionReportTransformer();

			foreach (var permissionHolders in reportPermissions.Batch(100000))
			{
				BulkInsertDataTable1.Rows.Clear();
				transformer.Transform(permissionHolders, BulkInsertDataTable1);
				affectedRows += _jobParameters.Helper.Repository.PersistPermissionReport(BulkInsertDataTable1);
			}

			return affectedRows;
		}
	}
}