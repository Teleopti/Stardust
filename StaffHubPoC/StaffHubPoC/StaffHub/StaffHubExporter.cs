using System;
using System.Collections.Generic;
using System.Linq;
using StaffHubPoC.Models;

namespace StaffHubPoC.StaffHub
{
	public class StaffHubExporter
	{
		private ShiftCollection _allShifts;
		private List<Member> _allMembers;
		private readonly Team _myTeam;
		private string _token;

		public StaffHubExporter()
		{
			Init();

			_myTeam = StaffHubCommunicator.GetMyTeam(_token);
			_allMembers = StaffHubCommunicator.GetMembers(_myTeam, _token);
			_allShifts = StaffHubCommunicator.GetAllShifts(_myTeam, _token);
		}

		public void PostShifts(List<Shift> shifts)
		{
			foreach (var shift in shifts)
			{
				var overlappingPublishedShifts =
					_allShifts.PublishedShifts.Where(x => x.startTime < shift.endTime && x.endTime > x.startTime);
				var overlappingUnpublishedShifts =
					_allShifts.UnpublishedShifts.Where(x => x.startTime < shift.endTime && x.endTime > x.startTime);
				if (overlappingPublishedShifts.Any()) continue;
				foreach (var overlappingUnpublishedShift in overlappingUnpublishedShifts)
				{
					StaffHubCommunicator.DeleteShift(_myTeam, overlappingUnpublishedShift, _token);
				}

				StaffHubCommunicator.PostShift(shift, _myTeam, _token);
			}
		}

		private void Init()
		{
			var clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
			var redirectUrl = new Uri(System.Configuration.ConfigurationManager.AppSettings["redirectUrl"]);
			const string resourceId = "aa580612-c342-4ace-9055-8edee43ccb89";
			var authority = "https://login.microsoftonline.com/teleopti.onmicrosoft.com/oauth2/token";
			var authorizeUri = "https://login.microsoftonline.com/teleopti.onmicrosoft.com/oauth2/authorize";

			_token = OAuthFlow.Flow(
				clientId,
				resourceId,
				redirectUrl,
				authority,
				authorizeUri,
				true,
				"KIxOydAoOQDBtkEk9WWWo8WqIH6K5h00AuJ0pqBLfOE=");
		}
	}
}
