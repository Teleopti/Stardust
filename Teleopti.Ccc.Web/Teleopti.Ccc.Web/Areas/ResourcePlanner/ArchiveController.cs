using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public ArchiveController(IPersonRepository personRepository, IEventPopulatingPublisher eventPublisher, IToggleManager toggleManager, ITeamRepository teamRepository, IPermissionProvider permissionProvider, IJobResultRepository jobResultRepository, ILoggedOnUser loggedOnUser)
		{
			_personRepository = personRepository;
			_eventPublisher = eventPublisher;
			_toggleManager = toggleManager;
			_teamRepository = teamRepository;
			_permissionProvider = permissionProvider;
			_jobResultRepository = jobResultRepository;
			_loggedOnUser = loggedOnUser;
		}

		[Route("api/ResourcePlanner/Importing/Run"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult RunImporting([FromBody] ArchiveSchedulesModel model)
		{
			var archiveSchedulesResponse = new ArchiveSchedulesResponse
			{
				TotalMessages = 0,
				TotalSelectedPeople = 0
			};
			if (_toggleManager.IsEnabled(Toggles.Wfm_ImportSchedule_41247))
			{
				var people = getPeople(model);
				archiveSchedulesResponse.TotalSelectedPeople = people.Count;
				if (archiveSchedulesResponse.TotalSelectedPeople > 0)
				{
					var jobResult = new JobResult(JobCategory.ImportSchedule, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _loggedOnUser.CurrentUser(), DateTime.UtcNow);
					_jobResultRepository.Add(jobResult);
					archiveSchedulesResponse.JobId = jobResult.Id;
					model.JobResultId = jobResult.Id.GetValueOrDefault();

					var importScheduleEvents = people
					   .Batch(getBatchSize(archiveSchedulesResponse.TotalSelectedPeople))
					   .Select(model.CreateImportEvent)
					   .Where(x => x.PersonIds.Any())
					   .ToList();
					importScheduleEvents.ForEach(e => e.TotalMessages = importScheduleEvents.Count);

					Task.Run(() => _eventPublisher.Publish(importScheduleEvents.Cast<IEvent>().ToArray()));
					archiveSchedulesResponse.TotalMessages = importScheduleEvents.Count;
				}
			}
			return Ok(archiveSchedulesResponse);
		}

		[Route("api/ResourcePlanner/Archiving/Run"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult RunArchiving([FromBody] ArchiveSchedulesModel model)
		{
			var archiveSchedulesResponse = new ArchiveSchedulesResponse
			{
				TotalMessages = 0,
				TotalSelectedPeople = 0
			};
			if (_toggleManager.IsEnabled(Toggles.Wfm_ArchiveSchedule_41498))
			{
				var people = getPeople(model);
				archiveSchedulesResponse.TotalSelectedPeople = people.Count;
				if (archiveSchedulesResponse.TotalSelectedPeople > 0)
				{
					var jobResult = new JobResult(JobCategory.ArchiveSchedule, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _loggedOnUser.CurrentUser(), DateTime.UtcNow);
					_jobResultRepository.Add(jobResult);
					archiveSchedulesResponse.JobId = jobResult.Id;
					model.JobResultId = jobResult.Id.GetValueOrDefault();

					var archiveScheduleEvents = people
						.Batch(getBatchSize(archiveSchedulesResponse.TotalSelectedPeople))
						.Select(model.CreateArchiveEvent)
						.Where(x => x.PersonIds.Any())
						.ToList();
					archiveScheduleEvents.ForEach(e => e.TotalMessages = archiveScheduleEvents.Count);

					Task.Run(() => _eventPublisher.Publish(archiveScheduleEvents.Cast<IEvent>().ToArray()));
					archiveSchedulesResponse.TotalMessages = archiveScheduleEvents.Count;
				}
			}
			return Ok(archiveSchedulesResponse);
		}

		[Route("api/ResourcePlanner/JobStatus/{jobId}"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult JobResult(Guid jobId)
		{
			var jobResult = _jobResultRepository.FindWithNoLock(jobId);
			if (jobResult != null)
				return Ok(new { Successful=jobResult.Details.Count(x => x.DetailLevel == DetailLevel.Info && x.ExceptionMessage == null)});
			return NotFound();
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
			return Math.Max(totalPeople / 150, 5);
		}
	}
}