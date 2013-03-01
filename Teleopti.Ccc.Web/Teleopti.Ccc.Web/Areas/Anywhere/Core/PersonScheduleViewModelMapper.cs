using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMapper : IPersonScheduleViewModelMapper
	{
		public PersonScheduleViewModel Map(PersonScheduleData data)
		{
			var team = data.Person != null ? data.Person.MyTeam(new DateOnly(data.Date)) : null;

			var layers = new PersonScheduleViewModelLayer[] { };

			if (data.Shift != null)
				layers = (from d in data.Shift.Projection as IEnumerable<dynamic>
			        select new PersonScheduleViewModelLayer
				        {
					        Color = d.Color
				        }).ToArray();

			var viewModel = new PersonScheduleViewModel
				{
					Name = data.Person != null ? data.Person.Name.ToString() : null,
					Team = team == null ? null : team.Description.Name,
					Site = team == null || team.Site == null ? null : team.Site.Description.Name,
					Layers = layers
				};
			return viewModel;
		}
	}
}