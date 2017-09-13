using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Payroll
{
	public class RunPayrollExportEvent : StardustJobInfo
	{
		public RunPayrollExportEvent()
		{
			ExportPersonIdCollection = new Collection<Guid>();
		}

		public Guid PayrollExportId { get; set; }

		public Guid OwnerPersonId { get; set; }

		public DateTime ExportStartDate { get; set; }
		public DateTime ExportEndDate { get; set; }
		public Guid PayrollExportFormatId { get; set; }

		public ICollection<Guid> ExportPersonIdCollection { get; set; }

		public Guid PayrollResultId { get; set; }

	}
}