using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Shared
{
	public class DateTimePeriodFormValidator
	{
		public IEnumerable<ValidationResult> Validate(DateTimePeriodForm form)
		{
			var startDate = form.StartDate.Date;
			var endDate = form.EndDate.Date;

			if (startDate.Date.Add(form.StartTime.Time).CompareTo(endDate.Date.Add(form.EndTime.Time)) >= 0)
				yield return new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.Period));

			if (!isValidDateRange(startDate))
				yield return new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.StartDate));

			if (!isValidDateRange(endDate))
				yield return new ValidationResult(string.Format(Resources.InvalidTimeValue, Resources.EndDate));
		}

		private static bool isValidDateRange(DateTime date)
		{
			return date > DateHelper.MinSmallDateTime && date < DateHelper.MaxSmallDateTime;
		}
	}
}