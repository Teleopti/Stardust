using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPeople)]
	public class PeopleSmartWorkflowControlSetController : ApiController
	{
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;
		private readonly ISmartPersonPropertyQuerier _smartPersonPropertyQuerier;
		private readonly IDisableDeletedFilter _disableDeletedFilter;

		public PeopleSmartWorkflowControlSetController(IWorkflowControlSetRepository workflowControlSetRepository,
			IPersonRepository personRepository, INow now, ISmartPersonPropertyQuerier smartPersonPropertyQuerier,
			IDisableDeletedFilter disableDeletedFilter)
		{
			_workflowControlSetRepository = workflowControlSetRepository;
			_personRepository = personRepository;
			_now = now;
			_smartPersonPropertyQuerier = smartPersonPropertyQuerier;
			_disableDeletedFilter = disableDeletedFilter;
		}

		[HttpPost, UnitOfWork, Route("api/People/SmartWorkflowControlSet/Suggestion")]
		public virtual IHttpActionResult GetSuggestions([FromBody]Guid[] personIds)
		{
			var model = new SmartAssignWorkflowControlSetModel();
			IList<IWorkflowControlSet> workflowControlSets;
			using (_disableDeletedFilter.Disable())
			{
				workflowControlSets = _workflowControlSetRepository.LoadAll();
			}

			var localDateOnly = _now.ServerDate_DontUse();
			model.WorkflowControlSets =
				workflowControlSets
					.Select(w => new WorkflowControlSetModel { Id = w.Id.GetValueOrDefault(), Name = w.Name, IsDeleted = ((IDeleteTag)w).IsDeleted })
					.ToArray();
			var workflowControlSetSuggestions = _smartPersonPropertyQuerier.GetWorkflowControlSetSuggestions().ToLookup(w => w.Team);
			model.Suggestions =
				_personRepository.FindPeople(personIds)
					.Select(
						p =>
						{
							var team = p.Period(localDateOnly)?.Team;
							var suggestedId = Guid.Empty;
							var confidence = 0d;
							var teamId = team?.Id;
							if (teamId.HasValue)
							{
								var suggestions = workflowControlSetSuggestions[teamId.Value].ToArray();
								var sum = suggestions.Sum(s => s.Priority);
								var suggestion = suggestions.FirstOrDefault(
									w => model.WorkflowControlSets.Any(wcs => wcs.Id == w.WorkflowControlSet && !wcs.IsDeleted));
								suggestedId =
									suggestion
										.WorkflowControlSet;
								if (sum > 0)
								{
									confidence = (double)suggestion.Priority/sum;
								}
							}
							if (suggestedId == Guid.Empty)
							{
								suggestedId = model.WorkflowControlSets.FirstOrDefault(w => !w.IsDeleted)?.Id ?? Guid.Empty;
							}
							return new SmartAssignSuggestionModel
							{
								PersonId = p.Id.GetValueOrDefault(),
								FirstName = p.Name.FirstName,
								LastName = p.Name.LastName,
								CurrentId = p.WorkflowControlSet?.Id,
								SuggestedId = suggestedId,
								CurrentTeam = team?.Description.Name,
								CurrentSite = team?.Site?.Description.Name,
								Confidence = confidence
							};
						})
					.ToArray();

			return Ok(model);
		}

		[HttpPut, UnitOfWork, Route("api/People/SmartWorkflowControlSet")]
		public IHttpActionResult ApplyWorkflowControlSet([FromBody]ApplySmartPropertyModel[] model)
		{
			var allPeopleId = model.Select(m => m.PersonId);
			var allWorkflowControlSets = _workflowControlSetRepository.LoadAll().ToDictionary(k => k.Id.GetValueOrDefault());
			var people = _personRepository.FindPeople(allPeopleId).ToDictionary(k => k.Id.GetValueOrDefault());

			foreach (var propertyModel in model)
			{
				IWorkflowControlSet wcs;
				IPerson person;
				if (allWorkflowControlSets.TryGetValue(propertyModel.ApplyId, out wcs) &&
					people.TryGetValue(propertyModel.PersonId, out person))
				{
					person.WorkflowControlSet = wcs;
				}
			}

			return Ok();
		}
	}
}