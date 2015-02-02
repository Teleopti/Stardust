using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatPlanCommand : ITrackableCommand
	{
		public IList<Guid> Teams { get; set; }
		public IList<Guid> Locations { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ValidationResult { get; set; }

		// RobTodo: Remove this - throwaway, just for prototype
		public dynamic LocationsFromFile { get; set; }

		public bool IsValid()
		{
			var isValid = true;
			ValidationResult = new List<string>();
			if (StartDate > EndDate)
			{
				isValid = false;
				ValidationResult.Add(Resources.StartDateMustBeSmallerThanEndDate);
			}
			return isValid;
		}
		
	}
}