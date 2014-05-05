using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
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

		public bool IsPermittedToViewMeeting(ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView)
		{
			const string functionPath = DefinedRaptorApplicationFunctionPaths.ModifyMeetings;
			return CheckPermission(functionPath, scheduleView, selectedEntitiesFromTreeView);
		}

		public bool IsPermittedToEditMeeting(ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView, IScenario scenario)
		{
			if (!IsPermittedToViewMeeting(scheduleView, selectedEntitiesFromTreeView)) return false;
			return !scenario.Restricted || PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);
		}

		public bool IsPermittedToWriteProtect(ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView)
		{
			const string functionPath = DefinedRaptorApplicationFunctionPaths.SetWriteProtection;
			return CheckPermission(functionPath, scheduleView, selectedEntitiesFromTreeView);
		}

		public bool IsThisPermittedApproveRequest(PersonRequestViewModel model)
		{
			return new PersonRequestAuthorization(PrincipalAuthorization.Instance()).IsPermittedRequestApprove(model.RequestType);
		}

		public bool IsPermittedApproveRequest(IEnumerable<PersonRequestViewModel> models)
		{
			foreach (var model in models)
			{
				if (!IsThisPermittedApproveRequest(model)) return false;
			}
			return true;
		}

		public bool IsPermittedViewRequest()
		{
			return new PersonRequestAuthorization(PrincipalAuthorization.Instance()).IsPermittedRequestView();
		}

		public bool IsViewRequestDetailsAvailable(RequestView requestView)
		{
			if (requestView.SelectedAdapters() == null || requestView.SelectedAdapters().Count != 1)
				return false;
			var selectedModel = requestView.SelectedAdapters().First();
			if (!selectedModel.IsWithinSchedulePeriod)
				return false;
			if (!IsPermittedViewRequest())
				return false;
			return true;
		}

		public bool IsPermittedToViewSchedules(IEnumerable selectedEntitiesFromTreeView)
		{
			if (HasFunctionPermissionForTeams(selectedEntitiesFromTreeView.OfType<ITeam>(), DefinedRaptorApplicationFunctionPaths.ViewSchedules)) return true;
			return false;
		}
	}
}
