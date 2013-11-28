using System;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateOnly Date { get; set; }
		public TimeOfDay StartTime { get; set; }
		public TimeOfDay EndTime { get; set; }

		public IList<string> ValidationResult { get; set; }

		public bool IsValid()
		{
			var isValid = true;
			ValidationResult = new List<string>();
			if (StartTime.Time > EndTime.Time)
			{
				isValid = false;
				ValidationResult.Add(Resources.InvalidEndTime);
			}
			return isValid;
		}
	}
}