using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StaffHubPoC.Models;
using StaffHubPoC.Types;

namespace StaffHubPoC
{
	public static class Communicator
	{
		public static Team GetMyTeam(string token)
		{
			var jsonResultTeams = HttpSender.Get("api/beta/users/me/teams", token);
			var team = JsonConvert.DeserializeObject<TeamCollection>(jsonResultTeams).teams.First();
			return team;
		}

		public static IList<Member> GetMembers(Team team, string token)
		{
			var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/members", token);
			var members = JsonConvert.DeserializeObject<MemberCollection>(jsonResult).members;
			return members;
		}

		//public static Shift GetShift(string token)
		//{
		//	var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/members", token);
		//}

		public static Shift PostShift(Member member, DateTime startDateTime, DateTime endDateTime, List<Break> breaks, string shiftTitle, Team team, string token)
		{
			var shift = new Shift
			{
				title = shiftTitle,
				breaks = breaks,
				startTime = startDateTime,
				endTime = endDateTime,
				groupIds = member.groupIds,
				memberId = member.id,
				notes = "Imported from Teleopti WFM",
				shiftType = ShiftType.Working,
				state = StateType.Active,
				isPublished = true
			};

			var payload = JsonConvert.SerializeObject(shift);
			var jsonResult = HttpSender.Post($"api/beta/teams/{team.id}/shifts", payload, token);
			var createdShift = JsonConvert.DeserializeObject<ShiftCollection>(jsonResult);
			return createdShift.shift;
		}

		
	}
}
