using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StaffHubPoC.Models;
using StaffHubPoC.Util;

namespace StaffHubPoC.StaffHub
{
	public static class StaffHubCommunicator
	{
		public static Team GetMyTeam(string token)
		{
			var jsonResult = HttpSender.Get("api/beta/users/me/teams", token);
			if (jsonResult == null) return null;
			var team = JsonConvert.DeserializeObject<TeamCollection>(jsonResult).teams.First();
			return team;
		}

		public static List<Member> GetMembers(Team team, string token)
		{
			var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/members", token);
			if (jsonResult == null) return null;
			var members = JsonConvert.DeserializeObject<MemberCollection>(jsonResult).members;
			return members;
		}

		public static List<Group> GetGroups(Team team, string token)
		{
			var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/groups", token);
			if (jsonResult == null) return null;
			var groups = JsonConvert.DeserializeObject<GroupCollection>(jsonResult).groups;
			return groups;
		}

		public static ShiftCollection GetAllShifts(Team team, string token)
		{
			var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/shifts", token);
			if (jsonResult == null) return null;
			var allShifts = JsonConvert.DeserializeObject<ShiftCollection>(jsonResult);
			return allShifts;
		}

		public static Shift GetShift(Team team, Shift shift, string token)
		{
			var jsonResult = HttpSender.Get($"api/beta/teams/{team.id}/shifts/{shift.id}", token);
			if (jsonResult == null) return null;
			var shiftFresh = JsonConvert.DeserializeObject<ShiftRoot>(jsonResult).shift;
			return shiftFresh;
		}

		public static Shift DeleteShift(Team team, Shift shift, string token)
		{
			var payload = $"{{\"eTag\": {shift.eTag}}}";
			var jsonResult = HttpSender.Delete($"api/beta/teams/{team.id}/shifts/{shift.id}", payload, token);
			if (jsonResult == null) return null;
			var deletedShift = JsonConvert.DeserializeObject<Shift>(jsonResult);
			return deletedShift;
		}

		public static Shift PostShift(Shift shift, Team team, string token)
		{
			var payload = JsonConvert.SerializeObject(shift);
			var jsonResult = HttpSender.Post($"api/beta/teams/{team.id}/shifts", payload, token);
			if (jsonResult == null) return null;
			var createdShift = JsonConvert.DeserializeObject<ShiftRoot>(jsonResult);
			return createdShift.shift;
		}

		
	}
}
