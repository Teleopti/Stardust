using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.ArchiveSchedule)]
	public class ArchiveController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IToggleManager _toggleManager;
		private readonly ITeamRepository _teamRepository;
		private readonly IPermissionProvider _permissionProvider;

		public ArchiveController(IPersonRepository personRepository, IEventPopulatingPublisher eventPublisher, IToggleManager toggleManager, ITeamRepository teamRepository, IPermissionProvider permissionProvider)
		{
			_personRepository = personRepository;
			_eventPublisher = eventPublisher;
			_toggleManager = toggleManager;
			_teamRepository = teamRepository;
			_permissionProvider = permissionProvider;
		}

		[Route("api/ResourcePlanner/Archiving/Run"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult RunArchiving([FromBody] ArchiveSchedulesModel model)
		{
			var totalMessages = 0;
			var totalPeople = 0;
			if (_toggleManager.IsEnabled(Toggles.Wfm_ArchiveSchedule_41498))
			{
				var people = getPeople(model);
				totalPeople = people.Count;
				var archiveScheduleEvents = createEvents(model, people, totalPeople);
				if (archiveScheduleEvents.Any())
				{
					Task.Run(() =>
					{
						_eventPublisher.Publish(archiveScheduleEvents);
					});
				}
				totalMessages = archiveScheduleEvents.Length;
			}
			return Ok(new ArchiveSchedulesResponse
			{
				TotalMessages = totalMessages,
				TotalSelectedPeople = totalPeople
			});
		}

		private static IEvent[] createEvents(ArchiveSchedulesModel model, HashSet<IPerson> people, int totalPeople)
		{
			return people
				.Batch(getBatchSize(totalPeople))
				.Select(
					batchedPeople => new ArchiveScheduleEvent(batchedPeople.Select(person => person.Id.GetValueOrDefault()).ToArray())
					{
						StartDate = model.StartDate,
						EndDate = model.EndDate,
						FromScenario = model.FromScenario,
						ToScenario = model.ToScenario,
						TrackingId = model.TrackId
					})
				.Where(x => x.PersonIds.Any())
				.Cast<IEvent>().ToArray();
		}

		private HashSet<IPerson> getPeople(ArchiveSchedulesModel model)
		{
			var teams = _teamRepository.FindTeams(model.SelectedTeams);
			var people = new HashSet<IPerson>();
			foreach (var team in teams)
			{
				if (!_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ArchiveSchedule, DateOnly.Today, team))
					throw new PermissionException("You do not have permission for all of the selected teams.");
				people.AddRange(_personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(new DateOnly(model.StartDate), new DateOnly(model.EndDate))));
			}
			return people;
		}

		private static int getBatchSize(int totalPeople)
		{
			return Math.Max(totalPeople/150, 5);
		}
	}
}