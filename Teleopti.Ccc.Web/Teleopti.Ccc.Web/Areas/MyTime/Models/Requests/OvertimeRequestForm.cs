using System;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class OvertimeRequestForm
	{
		public Guid? Id { get; set; }

		[Required(AllowEmptyStrings = false,
			ErrorMessageResourceType = typeof(Resources),
			ErrorMessageResourceName = "MissingSubject")]
		public string Subject { get; set; }
		public string Message { get; set; }

		[Required(AllowEmptyStrings = false,
			ErrorMessageResourceType = typeof(Resources),
			ErrorMessageResourceName = "MissingOvertimeType")]
		public Guid? MultiplicatorDefinitionSet { get; set; }
		public DateTimePeriodForm Period { get; set; }
	}
}