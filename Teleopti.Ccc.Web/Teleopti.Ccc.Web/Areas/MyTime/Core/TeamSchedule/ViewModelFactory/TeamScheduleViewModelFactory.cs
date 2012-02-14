using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IMappingEngine _mapper;
		private readonly ITeamProvider _teamProvider;

		public TeamScheduleViewModelFactory(IMappingEngine mapper, ITeamProvider teamProvider)
		{
			_mapper = mapper;
			_teamProvider = teamProvider;
		}

		public TeamScheduleViewModel CreateViewModel(DateOnly date, Guid id)
		{
			var domainData = _mapper.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(date, id));
			return _mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(domainData);
		}

		public IEnumerable<SelectBoxOption> CreateTeamOptionsViewModel(DateOnly date)
		{
			var teams = _teamProvider.GetPermittedTeams(date);
			var sites = teams
				.Select(t => t.Site)
				.Distinct()
				.OrderBy(s => s.Description.Name);

			var options = new List<SelectBoxOption>();
			sites.ForEach(s =>
			              	{
			              		options.Add(new SelectBoxOption
			              		            	{
			              		            		Value = "-",
			              		            		Text = s.Description.Name
			              		            	});
			              		var teamOptions = from t in teams
			              		                  where t.Site == s
			              		                  select new SelectBoxOption
			              		                         	{
			              		                         		Value = t.Id.Value.ToString(),
			              		                         		Text = t.Description.Name
			              		                         	};
			              		options.AddRange(teamOptions);
			              	});

			return options;
		}
	}
}