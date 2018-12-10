using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Shared
{
	public class DateTimePeriodForm : IValidatableObject 
	{
		public DateOnly StartDate { get; set; }
		public TimeOfDay StartTime { get; set; }
		public DateOnly EndDate { get; set; }
		public TimeOfDay EndTime { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return new DateTimePeriodFormValidator().Validate(this);
		}
	}
}