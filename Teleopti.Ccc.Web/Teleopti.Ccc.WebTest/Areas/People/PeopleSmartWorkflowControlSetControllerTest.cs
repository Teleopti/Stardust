using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
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
		public IDisableDeletedFilter DisableDeletedFilter;

		[Test]
		public void ShouldGetCurrentWorkflowControlSetWithSuggestion()
		{
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePerson().WithId();
			person.WorkflowControlSet = workflowControlSet;
			PersonRepository.Add(person);

			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now,
				new FakeSmartPersonPropertyQuerier(), DisableDeletedFilter);
			var peopleWithWorkflowControlSet =
				(OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)
				target.GetSuggestions(new[] {person.Id.GetValueOrDefault()});

			peopleWithWorkflowControlSet.Content.WorkflowControlSets.Length.Should().Be.EqualTo(1);
			var suggestionModel = peopleWithWorkflowControlSet.Content.Suggestions.Single();
			suggestionModel.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			suggestionModel.CurrentId.Should().Be.EqualTo(workflowControlSet.Id.GetValueOrDefault());
			suggestionModel.SuggestedId.Should().Be.EqualTo(workflowControlSet.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldGetSuggestedWorkflowControlSetWhenDeletedWorkflowControlSetExists()
		{
			var workflowControlSet1 = new WorkflowControlSet("OptimizationData").WithId();
			workflowControlSet1.SetDeleted();
			var workflowControlSet2 = new WorkflowControlSet("Test2").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet1);
			WorkflowControlSetRepository.Add(workflowControlSet2);
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Add(person);

			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now,
				new FakeSmartPersonPropertyQuerier(), DisableDeletedFilter);
			var peopleWithWorkflowControlSet =
				(OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)
				target.GetSuggestions(new[] { person.Id.GetValueOrDefault() });

			peopleWithWorkflowControlSet.Content.WorkflowControlSets.Length.Should().Be.EqualTo(2);
			var suggestionModel = peopleWithWorkflowControlSet.Content.Suggestions.Single();
			suggestionModel.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			suggestionModel.CurrentId.HasValue.Should().Be.False();
			suggestionModel.SuggestedId.Should().Be.EqualTo(workflowControlSet2.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldIncludeTeamAndSiteWithSuggestion()
		{
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2001,1,1)).WithId();
			person.WorkflowControlSet = workflowControlSet;
			person.Period(new DateOnly(2001, 1, 1)).Team.WithId();
			PersonRepository.Add(person);

			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now,
				new FakeSmartPersonPropertyQuerier(), DisableDeletedFilter);
			var peopleWithWorkflowControlSet =
				(OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)
				target.GetSuggestions(new[] {person.Id.GetValueOrDefault()});

			peopleWithWorkflowControlSet.Content.WorkflowControlSets.Length.Should().Be.EqualTo(1);
			var suggestionModel = peopleWithWorkflowControlSet.Content.Suggestions.Single();
			suggestionModel.CurrentTeam.Should().Be.EqualTo("Team 1");
			suggestionModel.CurrentSite.Should().Be.EqualTo("Site");
		}

		[Test]
		public void ShouldUseSuggestionBasedOnCommonControlSetInTeamFirstHand()
		{
			var defaultWorkflowControlSet = new WorkflowControlSet("Default").WithId();
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			WorkflowControlSetRepository.Add(defaultWorkflowControlSet);
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2001, 1, 1)).WithId();
			person.WorkflowControlSet = defaultWorkflowControlSet;
			PersonRepository.Add(person);

			var personPeriod = person.Period(new DateOnly(2001, 1, 1));
			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now,
				new FakeSmartPersonPropertyQuerier().WithSuggestion(new SmartPersonPropertySuggestion
				{
					Priority = 1,
					Team = personPeriod.Team.WithId().Id.GetValueOrDefault(),
					WorkflowControlSet = workflowControlSet.Id.GetValueOrDefault()
				}), DisableDeletedFilter);

			var peopleWithWorkflowControlSet =
				(OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)
				target.GetSuggestions(new[] {person.Id.GetValueOrDefault()});
			peopleWithWorkflowControlSet.Content.Suggestions.Single()
				.SuggestedId.Should()
				.Be.EqualTo(workflowControlSet.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldCalculaceAccuracyForSuggestion()
		{
			var defaultWorkflowControlSet = new WorkflowControlSet("Default").WithId();
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			WorkflowControlSetRepository.Add(defaultWorkflowControlSet);
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2001, 1, 1)).WithId();
			person.WorkflowControlSet = defaultWorkflowControlSet;
			PersonRepository.Add(person);

			var teamId = person.Period(new DateOnly(2001, 1, 1)).Team.WithId().Id.GetValueOrDefault();
			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now,
				new FakeSmartPersonPropertyQuerier().WithSuggestion(new SmartPersonPropertySuggestion
				{
					Priority = 3,
					Team = teamId,
					WorkflowControlSet = workflowControlSet.Id.GetValueOrDefault()
				}).WithSuggestion(new SmartPersonPropertySuggestion
				{
					Priority = 1,
					Team = teamId,
					WorkflowControlSet = defaultWorkflowControlSet.Id.GetValueOrDefault()
				}), DisableDeletedFilter);

			var peopleWithWorkflowControlSet =
				(OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)
				target.GetSuggestions(new[] { person.Id.GetValueOrDefault() });
			peopleWithWorkflowControlSet.Content.Suggestions.Single()
				.SuggestedId.Should()
				.Be.EqualTo(workflowControlSet.Id.GetValueOrDefault());
			peopleWithWorkflowControlSet.Content.Suggestions.Single()
				.Confidence.Should()
				.Be.EqualTo(0.75);
		}

		[Test]
		public void ShouldExcludeDeletedWorkflowControlSetFromSuggestion()
		{
			var defaultWorkflowControlSet = new WorkflowControlSet("Default").WithId();
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			workflowControlSet.SetDeleted();
			WorkflowControlSetRepository.Add(defaultWorkflowControlSet);
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2001, 1, 1)).WithId();
			person.WorkflowControlSet = defaultWorkflowControlSet;
			PersonRepository.Add(person);

			var personPeriod = person.Period(new DateOnly(2001, 1, 1));
			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now,
				new FakeSmartPersonPropertyQuerier().WithSuggestion(new SmartPersonPropertySuggestion
				{
					Priority = 1,
					Team = personPeriod.Team.WithId().Id.GetValueOrDefault(),
					WorkflowControlSet = workflowControlSet.Id.GetValueOrDefault()
				}), DisableDeletedFilter);

			var peopleWithWorkflowControlSet =
				(OkNegotiatedContentResult<SmartAssignWorkflowControlSetModel>)
				target.GetSuggestions(new[] { person.Id.GetValueOrDefault() });
			peopleWithWorkflowControlSet.Content.Suggestions.Single()
				.SuggestedId.Should()
				.Be.EqualTo(defaultWorkflowControlSet.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldApplyWorkflowControlSetFromSuggestion()
		{
			var workflowControlSet = new WorkflowControlSet("Test").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2001, 1, 1)).WithId();
			PersonRepository.Add(person);

			var target = new PeopleSmartWorkflowControlSetController(WorkflowControlSetRepository, PersonRepository, Now,
				new FakeSmartPersonPropertyQuerier(), DisableDeletedFilter);

			target.ApplyWorkflowControlSet(new[]
			{
				new ApplySmartPropertyModel
				{
					PersonId = person.Id.GetValueOrDefault(),
					ApplyId = workflowControlSet.Id.GetValueOrDefault()
				}
			});
			person.WorkflowControlSet.Should().Be.EqualTo(workflowControlSet);
		}
	}
}