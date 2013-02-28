using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMapper : IPersonScheduleViewModelMapper
	{
		public PersonScheduleViewModel Map(PersonScheduleData data)
		{
			//{"Date":"2012-12-02T00:00:00","FirstName":"Pierre","LastName":"Baldi","EmploymentNumber":"","Id":"268c4c7c-94a1-48e8-845f-a17300f1ac86","ContractTimeMinutes":435.0,"WorkTimeMinutes":435.0,"Projection":[{"Color":"Green","Title":"Phone","Start":"2012-12-02T20:00:00Z","End":"2012-12-02T23:30:00Z","Minutes":210.0},{"Color":"Yellow","Title":"Lunch","Start":"2012-12-02T23:30:00Z","End":"2012-12-03T00:15:00Z","Minutes":45.0},{"Color":"Green","Title":"Phone","Start":"2012-12-03T00:15:00Z","End":"2012-12-03T04:00:00Z","Minutes":225.0}]}

			var team = data.Person.MyTeam(new DateOnly(data.Date));
			var viewModel = new PersonScheduleViewModel
				{
					Name = data.Person.Name.ToString(),
					Team = team.Description.Name,
					Site = team.Site.Description.Name
				};
			return viewModel;
		}
	}
}