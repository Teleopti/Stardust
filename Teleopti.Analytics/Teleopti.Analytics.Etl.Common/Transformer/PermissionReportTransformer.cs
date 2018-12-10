using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Matrix;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class PermissionReportTransformer : IEtlTransformer<MatrixPermissionHolder>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<MatrixPermissionHolder> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);
			var list = rootList.ToList();
			table.MinimumCapacity = list.Count();

			foreach (var permissionHolder in list)
			{
				var reportId = new Guid(permissionHolder.ApplicationFunction.ForeignId);

				var row = table.NewRow();
				row["person_code"] = permissionHolder.Person.Id;
				row["ReportId"] = reportId;
				row["team_id"] = permissionHolder.Team.Id;
				row["business_unit_code"] = permissionHolder.Team.BusinessUnitExplicit.Id;
				row["business_unit_name"] = permissionHolder.Team.BusinessUnitExplicit.Name;
				row["my_own"] = permissionHolder.IsMy;
				row["datasource_id"] = 1;
				table.Rows.Add(row);
			}
		}

	}
}
