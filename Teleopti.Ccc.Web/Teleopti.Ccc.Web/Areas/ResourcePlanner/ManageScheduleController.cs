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
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.CopySchedule, DefinedRaptorApplicationFunctionPaths.ImportSchedule)]
	public class ManageScheduleController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly ITeamRepository _teamRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public ManageScheduleController(IPersonRepository personRepository, IEventPopulatingPublisher eventPublisher, ITeamRepository teamRepository, IPermissionProvider permissionProvider, IJobResultRepository jobResultRepository, ILoggedOnUser loggedOnUser)
		{
			_personRepository = personRepository;
			_eventPublisher = eventPublisher;
			_teamRepository = teamRepository;
			_permissionProvider = permissionProvider;
			_jobResultRepository = jobResultRepository;
			_loggedOnUser = loggedOnUser;
		}

		[Route("api/ResourcePlanner/Importing/Run"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult RunImporting([FromBody] ManageSchedulesModel model)
		{
			return ManageSchedules(model, JobCategory.ImportSchedule, model.CreateImportEvent<ImportScheduleEvent>, DefinedRaptorApplicationFunctionPaths.ImportSchedule);
		}

		[Route("api/ResourcePlanner/Copying/Run"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult RunCopying([FromBody] ManageSchedulesModel model)
		{
			return ManageSchedules(model, JobCategory.CopySchedule, model.CreateImportEvent<CopyScheduleEvent>, DefinedRaptorApplicationFunctionPaths.CopySchedule);
		}

		public IHttpActionResult ManageSchedules(ManageSchedulesModel model, string jobCategory, Func<IEnumerable<IPerson>, ManageScheduleBaseEvent> eventCreator, string requiredPermission)
		{
			var response = new ManageSchedulesResponse
			{
				TotalMessages = 0,
				TotalSelectedPeople = 0
			};
			var people = getPeople(model, requiredPermission);
			response.TotalSelectedPeople = people.Count;
			if (response.TotalSelectedPeople > 0)
			{
				var jobResult = new JobResult(jobCategory, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _loggedOnUser.CurrentUser(), DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				response.JobId = jobResult.Id;
				model.JobResultId = jobResult.Id.GetValueOrDefault();

				var events = people
					.Batch(getBatchSize(response.TotalSelectedPeople))
					.Select(eventCreator)
					.Where(x => x.PersonIds.Any())
					.ToList();
				events.ForEach(e => e.TotalMessages = events.Count);

				Task.Run(() => _eventPublisher.Publish(events.Cast<IEvent>().ToArray()));
				response.TotalMessages = events.Count;
			}
			return Ok(response);
		}

		[Route("api/ResourcePlanner/JobStatus/{jobId}"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult JobResult(Guid jobId)
		{
			var jobResult = _jobResultRepository.FindWithNoLock(jobId);
			if (jobResult != null)
				return Ok(new { Successful=jobResult.Details.Count(x => x.DetailLevel == DetailLevel.Info && x.ExceptionMessage == null)});
			return NotFound();
		}

		private HashSet<IPerson> getPeople(ManageSchedulesModel model, string requiredPermission)
		{
			var teams = _teamRepository.FindTeams(model.SelectedTeams);
			var people = new HashSet<IPerson>();
			foreach (var team in teams)
			{
				if (!_permissionProvider.HasTeamPermission(requiredPermission, DateOnly.Today, team))
					throw new PermissionException("You do not have permission for all of the selected teams.");
				people.AddRange(_personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(model.StartDate, model.EndDate)));
			}
			return people;
		}

		private static int getBatchSize(int totalPeople)
		{
			return Math.Max(totalPeople / 150, 5);
		}
	}
}