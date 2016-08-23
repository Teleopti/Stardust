using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class WarningInfo
	{
		public string RuleType { get; set; }
		public string Content { get; set; }
	}

	public class BusinessRuleValidationResult
	{
		public Guid PersonId;
		public List<WarningInfo> Warnings;
	}

	public class FetchRuleValidationResultFormData
	{
		public DateTime Date;
		public Guid[] PersonIds;
	}
}