using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623)]
	public class UpdatePersonGroupsAnalyticsHandler :
		IHandleEvent<PersonCollectionChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IPersonRepository _personRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;

		public UpdatePersonGroupsAnalyticsHandler(IPersonRepository personRepository, IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository)
		{
			_personRepository = personRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
		}

		public void Handle(PersonCollectionChangedEvent @event)
		{
			foreach (var personId in @event.PersonIdCollection)
			{
				var person = _personRepository.Get(personId) ?? new Person();
				foreach (var personPeriod in person.PersonPeriodCollection)
				{
					var groupIds = new List<Guid>();

					handleSkills(personPeriod, groupIds);
					handleContract(personPeriod, groupIds);
					handleContractSchedule(personPeriod, groupIds);
					handlePartTimePercentage(personPeriod, groupIds);
					handleRuleSetBag(personPeriod, groupIds);
					handleNotes(person, groupIds, @event.LogOnBusinessUnitId);

					var deletedGroupIds = updatePersonGroups(personPeriod.Id.GetValueOrDefault(), groupIds);

					clearEmptyGroups(deletedGroupIds);
				}
				// Remove any group pages associated with deleted person periods or deleted persons
				_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, person.PersonPeriodCollection.Select(p => p.Id.GetValueOrDefault()).ToList());
			}
		}

		// Add/Remove groups for a person period
		private IEnumerable<Guid> updatePersonGroups(Guid personPeriodId, ICollection<Guid> groupIds)
		{
			var currentGroups =
				_analyticsBridgeGroupPagePersonRepository.GetBuiltInGroupPagesForPersonPeriod(personPeriodId)
					.ToList();

			var toBeDeleted = currentGroups.Where(g => !groupIds.Contains(g)).ToList();
			var toBeAdded = groupIds.Where(g => !currentGroups.Contains(g)).ToList();
			_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, toBeDeleted);
			_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, toBeAdded);
			return toBeDeleted;
		}

		// Removes any GroupPages in specified set that have no people mapped to them
		private void clearEmptyGroups(IEnumerable<Guid> toBeDeleted)
		{
			var groupsToDelete = new List<Guid>();
			foreach (var groupId in toBeDeleted)
			{
				var peopleInGroup = _analyticsBridgeGroupPagePersonRepository.GetBridgeGroupPagePerson(groupId);
				if (!peopleInGroup.Any())
					groupsToDelete.Add(groupId);
			}
			_analyticsGroupPageRepository.DeleteGroupPagesByGroupCodes(groupsToDelete);
		}

		private static void handleSkills(IPersonPeriod personPeriod, List<Guid> groupIds)
		{
			var skills = personPeriod.PersonSkillCollection.Select(s => s.Skill.Id.GetValueOrDefault());
			groupIds.AddRange(skills);
		}

		private static void handleContract(IPersonPeriod personPeriod, ICollection<Guid> groupIds)
		{
			var contract = personPeriod.PersonContract.Contract.Id.GetValueOrDefault();
			if (contract != Guid.Empty && personPeriod.PersonContract.Contract.IsChoosable)
				groupIds.Add(contract);
		}

		private static void handleContractSchedule(IPersonPeriod personPeriod, ICollection<Guid> groupIds)
		{
			var contractSchedule = personPeriod.PersonContract.ContractSchedule.Id.GetValueOrDefault();
			if (contractSchedule != Guid.Empty && personPeriod.PersonContract.ContractSchedule.IsChoosable)
				groupIds.Add(contractSchedule);
		}

		private static void handlePartTimePercentage(IPersonPeriod personPeriod, ICollection<Guid> groupIds)
		{
			var partTimePercentage = personPeriod.PersonContract.PartTimePercentage.Id.GetValueOrDefault();
			if (partTimePercentage != Guid.Empty && personPeriod.PersonContract.PartTimePercentage.IsChoosable)
				groupIds.Add(partTimePercentage);
		}

		private static void handleRuleSetBag(IPersonPeriod personPeriod, ICollection<Guid> groupIds)
		{
			if (personPeriod.RuleSetBag != null)
			{
				var ruleSetBag = personPeriod.RuleSetBag.Id.GetValueOrDefault();
				if (ruleSetBag != Guid.Empty && personPeriod.RuleSetBag.IsChoosable)
					groupIds.Add(ruleSetBag);
			}
		}

		private void handleNotes(IPerson person, ICollection<Guid> groupIds, Guid businessUnitId)
		{
			// Find a group for Note, or create if there isn't one
			if (!string.IsNullOrWhiteSpace(person.Note))
			{
				var groupName = formatNoteName(person.Note);
				var noteGroupPage = _analyticsGroupPageRepository.FindGroupPageByGroupName(groupName);
				if (noteGroupPage == null)
				{
					var noteGroupCode = _analyticsGroupPageRepository.FindGroupPageCodeByResourceKey("Note");
					noteGroupPage = new AnalyticsGroupPage
					{
						GroupName = groupName,
						GroupCode = person.Note.GenerateGuid(),
						BusinessUnitCode = businessUnitId,
						GroupIsCustom = false,
						GroupPageCode = noteGroupCode != Guid.Empty ? noteGroupCode : Guid.NewGuid(),
						GroupPageName = Resources.Note,
						GroupPageNameResourceKey = "Note"
					};
					_analyticsGroupPageRepository.AddGroupPage(noteGroupPage);
				}
				groupIds.Add(noteGroupPage.GroupCode);
			}
		}

		private static string formatNoteName(string note)
		{
			return note.Length > 50 ? string.Format("{0}..", note.Substring(0, 48)) : note;
		}
	}
}