using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Newtonsoft.Json;
using StaffHubPoC.Models;
using StaffHubPoC.Types;

namespace StaffHubPoC.StaffHub
{
	public static class StaffHubCommunicator
	{
		public static Team GetMyTeam(string token)
		{
			var jsonResultTeams = HttpSender.Get("api/beta/users/me/teams", token);
			var team = JsonConvert.DeserializeObject<TeamCollection>(jsonResultTeams).teams.First();
			return team;
		}

		public static List<Member> GetMembers(Team team, string token)
		{
			var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/members", token);
			var members = JsonConvert.DeserializeObject<MemberCollection>(jsonResult).members;
			return members;
		}

		public static ShiftCollection GetAllShifts(Team team, string token)
		{
			var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/shifts", token);
			var allShifts = JsonConvert.DeserializeObject<ShiftCollection>(jsonResult);
			return allShifts;
		}

		public static Shift DeleteShift(Team team, Shift shift,string token)
		{
			var payload = JsonConvert.SerializeObject(shift.eTag);
			var jsonResult = HttpSender.Delete($"api/beta/teams/{team.id}/Shifts/{shift.id}", payload, token);
			var deletedShift = JsonConvert.DeserializeObject<Shift>(jsonResult);
			return deletedShift;
		}

		public static Shift PostShift(Shift shift, Team team, string token)
		{
			//var shift = new Shift
			//{
			//	title = shiftTitle,
			//	breaks = breaks,
			//	startTime = startDateTime,
			//	endTime = endDateTime,
			//	groupIds = member.groupIds,
			//	memberId = member.id,
			//	notes = "Imported from Teleopti WFM",
			//	shiftType = ShiftType.Working,
			//	state = StateType.Active,
			//	isPublished = true
			//};

			var payload = JsonConvert.SerializeObject(shift);
			var jsonResult = HttpSender.Post($"api/beta/teams/{team.id}/shifts", payload, token);
			var createdShift = JsonConvert.DeserializeObject<ShiftRoot>(jsonResult);
			return createdShift.shift;
		}

		
	}
}
