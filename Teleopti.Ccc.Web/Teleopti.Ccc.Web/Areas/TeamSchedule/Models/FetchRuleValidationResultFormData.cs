using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	

	public class BusinessRuleValidationResult
	{
		public Guid PersonId;
		public List<string> Warnings;
	}

	public class FetchRuleValidationResultFormData
	{
		public DateTime Date;
		public Guid[] PersonIds;
	}
}