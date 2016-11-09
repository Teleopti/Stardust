using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ArchiveController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IToggleManager _toggleManager;
		private readonly ITeamRepository _teamRepository;

		public ArchiveController(IPersonRepository personRepository, IEventPopulatingPublisher eventPublisher, IToggleManager toggleManager, ITeamRepository teamRepository)
		{
			_personRepository = personRepository;
			_eventPublisher = eventPublisher;
			_toggleManager = toggleManager;
			_teamRepository = teamRepository;
		}

		[Route("api/ResourcePlanner/Archiving/Run"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult RunArchiving([FromBody] ArchiveSchedulesModel model)
		{
			var count = 0;
			if (_toggleManager.IsEnabled(Toggles.Wfm_ArchiveSchedule_41498))
			{
				var teams = _teamRepository.FindTeams(model.SelectedTeams);
				var people = new HashSet<IPerson>();
				foreach (var team in teams)
					people.AddRange(_personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(new DateOnly(model.StartDate), new DateOnly(model.EndDate))));
				//var people = _personRepository.LoadAll().ToList();
				var archiveScheduleEvents = people.Select(person => new ArchiveScheduleEvent
				{
					StartDate = model.StartDate,
					EndDate = model.EndDate,
					FromScenario = model.FromScenario,
					ToScenario = model.ToScenario,
					PersonId = person.Id.GetValueOrDefault(),
					TrackingId = model.TrackId
				}).Cast<IEvent>().ToArray();
				Task.Run(() =>
				{
					_eventPublisher.Publish(archiveScheduleEvents);
				});
				count = archiveScheduleEvents.Length;
			}
			return Ok(new ArchiveSchedulesResponse
			{
				TotalMessages = count
			});
		}
	}
}