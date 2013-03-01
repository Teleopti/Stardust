using System;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMapper : IPersonScheduleViewModelMapper
	{
		public PersonScheduleViewModel Map(PersonScheduleData data)
		{
			var team = data.Person.MyTeam(new DateOnly(data.Date));
			var viewModel = new PersonScheduleViewModel
				{
					Name = data.Person.Name.ToString(),
					Team = team.Description.Name,
					Site = team.Site.Description.Name,
					Layers = new[]
						{
							new
								{
									Color = "Green",
									Title = "Phone",
									Start = data.Date.Add(TimeSpan.FromHours(8)),
									End = data.Date.Add(TimeSpan.FromHours(11)),
									Minutes = 3*60
								},
							new
								{
									Color = "Yellow",
									Title = "Lunch",
									Start = data.Date.Add(TimeSpan.FromHours(11)),
									End = data.Date.Add(TimeSpan.FromHours(12)),
									Minutes = 1*60
								},
							new
								{
									Color = "Green",
									Title = "Phone",
									Start = data.Date.Add(TimeSpan.FromHours(12)),
									End = data.Date.Add(TimeSpan.FromHours(17)),
									Minutes = 5*60
								}

						}
				};
			return viewModel;
		}
	}
}