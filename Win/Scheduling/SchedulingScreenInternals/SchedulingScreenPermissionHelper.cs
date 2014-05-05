using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public class SchedulingScreenPermissionHelper
	{
		public bool CheckPermission(string functionPath, ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView)
		{
			var schedulePart = scheduleView.ViewGrid[scheduleView.ViewGrid.CurrentCell.RowIndex, scheduleView.ViewGrid.CurrentCell.ColIndex].CellValue as IScheduleDay;
			if (schedulePart != null)
			{
				if (schedulePart.Person.PersonPeriodCollection.Count > 0)
				{
					IList<ITeam> teams = (from personPeriod in schedulePart.Person.PersonPeriodCollection
										  where personPeriod.Period.Contains(schedulePart.DateOnlyAsPeriod.DateOnly)
										  select personPeriod.Team).ToList();
					var permission = HasFunctionPermissionForTeams(teams, functionPath);
					return permission;
				}
			}

			return HasFunctionPermissionForTeams(selectedEntitiesFromTreeView.OfType<ITeam>(), functionPath);
		}

		public bool HasFunctionPermissionForTeams(IEnumerable<ITeam> teams, string functionPath)
		{
			var authorization = PrincipalAuthorization.Instance();
			foreach (ITeam team in teams)
			{
				if (!authorization.IsPermitted(functionPath, DateOnly.Today, team))
				{
					return false;
				}
			}
			return true;
		}
	}
}
