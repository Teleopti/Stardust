using System;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class TextRequestForm
	{
		[Required(AllowEmptyStrings = false, 
			ErrorMessageResourceType = typeof(Resources), 
			ErrorMessageResourceName = "MissingSubject")]
		[StringLength(80, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "SubjectTooLong")]
		public string Subject { get; set; }

        [StringLength(2000, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MessageTooLong")]
		public string Message { get; set; }

		public DateTimePeriodForm Period { get; set; }

		public Guid? EntityId { get; set; }

		public bool FullDay { get; set; }
	}
}