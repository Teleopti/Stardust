using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Shared
{
	public class DateTimePeriodFormValidator
	{
		public IEnumerable<ValidationResult> Validate(DateTimePeriodForm form)
		{
			if (form.StartDate.Date.Add(form.StartTime.Time) > form.EndDate.Date.Add(form.EndTime.Time))
				yield return new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.Period));
		}
	}
}