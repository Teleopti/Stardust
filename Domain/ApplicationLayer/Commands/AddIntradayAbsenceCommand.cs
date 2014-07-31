using System;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommand : ITrackedCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public TrackedCommandInfo TrackedCommandInfo { get; set; }

		public IList<string> ValidationResult { get; set; }

		public bool IsValid()
		{
			var isValid = true;
			ValidationResult = new List<string>();
			if (StartTime > EndTime)
			{
				isValid = false;
				ValidationResult.Add(Resources.InvalidEndTime);
			}
			return isValid;
		}
	}
}