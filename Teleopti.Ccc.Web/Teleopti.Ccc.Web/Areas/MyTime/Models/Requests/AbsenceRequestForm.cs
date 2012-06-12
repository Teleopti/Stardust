using System;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class AbsenceRequestForm : TextRequestForm
	{
		[Required(ErrorMessageResourceType = typeof(Resources),
			ErrorMessageResourceName = "MissingAbsenceType")]
		public Guid AbsenceId { get; set; }
	}
}