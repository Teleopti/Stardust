using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.WebTest.Core.SeatPlanner.Provider
{
	internal class SeatManagementProviderTestUtils
	{
		
		public static SeatBooking CreateSeatBooking(IPerson person, DateOnly belongsToDate, DateTime startDateTime, DateTime endDateTime)
		{
			var seatBooking = new SeatBooking(person, belongsToDate, startDateTime, endDateTime);
			seatBooking.SetId(Guid.NewGuid());
			return seatBooking;
		}
		
		public static Team CreateTeam(String name)
		{
			var team = new Team().WithDescription(new Description(name));
			team.SetId(Guid.NewGuid());
			return team;
		}
	}
}