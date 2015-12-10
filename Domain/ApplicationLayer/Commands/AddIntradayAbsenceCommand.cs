using System;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommand : ITrackableCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public TrackedCommandInfo TrackedCommandInfo { get; set; }

		public string ValidationResult { get; set; }

		public bool IsValid()
		{
			var isValid = true;
			ValidationResult = String.Empty;
			if (StartTime > EndTime)
			{
				isValid = false;
				ValidationResult = Resources.InvalidEndTime;
			}
			return isValid;
		}
	}
}