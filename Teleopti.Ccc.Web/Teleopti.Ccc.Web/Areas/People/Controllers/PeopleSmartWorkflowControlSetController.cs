using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleSmartWorkflowControlSetController : ApiController
	{
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public PeopleSmartWorkflowControlSetController(IWorkflowControlSetRepository workflowControlSetRepository, IPersonRepository personRepository, INow now)
		{
			_workflowControlSetRepository = workflowControlSetRepository;
			_personRepository = personRepository;
			_now = now;
		}

		[HttpPost, UnitOfWork]
		public virtual IHttpActionResult GetSuggestions([FromBody]Guid[] personIds)
		{
			var model = new SmartAssignWorkflowControlSetModel();
			var workflowControlSets = _workflowControlSetRepository.LoadAll();
			model.WorkflowControlSets =
				workflowControlSets
					.Select(w => new WorkflowControlSetModel { Id = w.Id.GetValueOrDefault(), Name = w.Name, IsDeleted = ((IDeleteTag)w).IsDeleted })
					.ToArray();
			model.Suggestions =
				_personRepository.FindPeople(personIds)
					.Select(
						p =>
						{
							var team = p.Period(_now.LocalDateOnly())?.Team;
							return new SmartAssignSuggestionModel
							{
								PersonId = p.Id.GetValueOrDefault(),
								FirstName = p.Name.FirstName,
								LastName = p.Name.LastName,
								CurrentId = p.WorkflowControlSet?.Id,
								SuggestedId = workflowControlSets.FirstOrDefault()?.Id.GetValueOrDefault() ?? Guid.Empty,
								CurrentTeam = team?.Description.Name,
								CurrentSite = team?.Site?.Description.Name
							};
						})
					.ToArray();

			return Ok(model);
		}
	}
}