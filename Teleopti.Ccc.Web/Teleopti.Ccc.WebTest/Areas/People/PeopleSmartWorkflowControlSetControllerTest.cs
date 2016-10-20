using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People
{
	[DomainTest]
	public class PeopleSmartWorkflowControlSetControllerTest
	{
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakePersonRepository PersonRepository;
		public MutableNow Now;

		[Test]
		public void ShouldGetCurrentWorkflowControlSetWithSuggestion()
		{
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePerson().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Add(person);

			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now);
			var peopleWithWorkflowControlSet = (OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)target.GetSuggestions(new[] {person.Id.GetValueOrDefault()});

			peopleWithWorkflowControlSet.Content.WorkflowControlSets.Length.Should().Be.EqualTo(1);
			var suggestionModel = peopleWithWorkflowControlSet.Content.Suggestions.Single();
			suggestionModel.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			suggestionModel.CurrentId.Should().Be.EqualTo(workflowControlSet.Id.GetValueOrDefault());
			suggestionModel.SuggestedId.Should().Be.EqualTo(workflowControlSet.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldIncludeTeamAndSiteWithSuggestion()
		{
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2001,1,1)).WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Add(person);

			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now);
			var peopleWithWorkflowControlSet = (OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)target.GetSuggestions(new[] { person.Id.GetValueOrDefault() });

			peopleWithWorkflowControlSet.Content.WorkflowControlSets.Length.Should().Be.EqualTo(1);
			var suggestionModel = peopleWithWorkflowControlSet.Content.Suggestions.Single();
			suggestionModel.CurrentTeam.Should().Be.EqualTo("Team 1");
			suggestionModel.CurrentSite.Should().Be.EqualTo("Site");
		}
	}
}