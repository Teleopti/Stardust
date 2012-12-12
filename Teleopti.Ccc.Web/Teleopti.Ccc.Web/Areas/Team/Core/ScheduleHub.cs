using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Team.Core
{
	[HubName("scheduleHub")]
	public class ScheduleHub : Hub
	{
		private readonly IPersonScheduleDayReadModelRepository _personScheduleDayReadModelRepository;
		private readonly ITeamProvider _teamProvider;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public ScheduleHub(IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository, ITeamProvider teamProvider, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_teamProvider = teamProvider;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		[UnitOfWork]
		public object AvailableTeams(DateTime date)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				return new
				{
					Teams = _teamProvider.GetPermittedTeams(new DateOnly(date)).Select(t => new { t.Id, t.SiteAndTeam }).ToList()
				};
			}
		}

		[UnitOfWork]
		public IEnumerable<object> SubscribeTeamSchedule(Guid teamId, DateTime date)
		{
			var schedule = _personScheduleDayReadModelRepository.ForTeam(new DateOnly(date), teamId);
			return schedule.Select(s => Newtonsoft.Json.JsonConvert.DeserializeObject(s.Shift));
		}
	}
}