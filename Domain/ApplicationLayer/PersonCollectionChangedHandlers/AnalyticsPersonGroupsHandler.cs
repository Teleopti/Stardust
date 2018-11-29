using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class AnalyticsPersonGroupsHandler : 
		IHandleEvent<AnalyticsPersonCollectionChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsPersonGroupsHandler));
		private readonly IPersonRepository _personRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IGroupPageRepository _groupPageRepository;
	    private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;

		public AnalyticsPersonGroupsHandler(IPersonRepository personRepository, IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository, IGroupPageRepository groupPageRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository)
		{
			_personRepository = personRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_groupPageRepository = groupPageRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
		}

		[UnitOfWork, AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(AnalyticsPersonCollectionChangedEvent @event)
		{
			logger.Debug($"Handle AnalyticsPersonCollectionChangedEvent for {@event.SerializedPeople}");
			foreach (var personId in @event.PersonIdCollection)
			{
				var person = _personRepository.Get(personId) ?? new Person();
				var personPeriod = person.Period(DateOnly.Today);
				var analyticsPersonPeriods = _analyticsPersonPeriodRepository.GetPersonPeriods(personId);
				var analyticsPersonPeriodIds = analyticsPersonPeriods.Select(x => x.PersonId).ToList();
				if (personPeriod != null)
				{
					var analyticsPersonPeriod = analyticsPersonPeriods.FirstOrDefault(x => x.PersonPeriodCode == personPeriod.Id.GetValueOrDefault());
					if (analyticsPersonPeriod == null)
						throw new PersonPeriodMissingInAnalyticsException(personPeriod.Id.GetValueOrDefault());
					var groupPages = _analyticsGroupPageRepository.GetBuildInGroupPageBase(analyticsPersonPeriod.BusinessUnitCode).ToList();
					var groupIds = new List<analyticsGroupForPerson>();

					handleSkills(personPeriod, groupIds, groupPages);
					handleContract(personPeriod, groupIds, groupPages);
					handleContractSchedule(personPeriod, groupIds, groupPages);
					handlePartTimePercentage(personPeriod, groupIds, groupPages);
					handleRuleSetBag(personPeriod, groupIds, groupPages);
					handleNotes(person.Note, groupIds, groupPages);
					handleCustomGroups(personId, groupIds);
					
					foreach (var analyticsPersonId in analyticsPersonPeriodIds)
					{
						var deletedGroupIds = updatePersonGroups(analyticsPersonId, groupIds, analyticsPersonPeriod.BusinessUnitCode);
						clearEmptyGroups(deletedGroupIds, analyticsPersonPeriod.BusinessUnitCode);
						// Remove any group pages associated with deleted person periods or deleted persons
					}
				}
				
				logger.Debug($"Deleting bridge group page person {personId} excluding person periods {string.Join(",", analyticsPersonPeriodIds)}");
				_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, analyticsPersonPeriodIds);
			}
		}

		private void handleCustomGroups(Guid personId, ICollection<analyticsGroupForPerson> groupIds)
		{
			var groupPages = _groupPageRepository.GetGroupPagesForPerson(personId);
			foreach (var groupPage in groupPages)
			{
				foreach (var rootGroup in groupPage.RootGroupCollection.Where(x => x.PersonCollection.Any(y => y.Id.GetValueOrDefault() == personId)))
				{
					groupIds.Add(new analyticsGroupForPerson(rootGroup.Id.GetValueOrDefault(), rootGroup.Name, new AnalyticsGroupPage
					{
						GroupPageCode = groupPage.Id.GetValueOrDefault(),
						GroupPageNameResourceKey = groupPage.DescriptionKey,
						GroupPageName = groupPage.Description.Name,
					}) {IsCustom = true});
				}
				
			}
		}

		// Add/Remove groups for a person period
		private IEnumerable<Guid> updatePersonGroups(int personId, ICollection<analyticsGroupForPerson> groups, Guid businessUnitId)
		{
			var currentGroups = _analyticsBridgeGroupPagePersonRepository.GetGroupPagesForPersonPeriod(personId, businessUnitId).ToList();

			var toBeDeleted = currentGroups.Where(g => groups.All(t => t.GroupCode != g)).ToList();
			var toBeAdded = groups.Where(t => currentGroups.All(g => g != t.GroupCode)).ToList();
			
			_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePersonForPersonPeriod(personId, toBeDeleted, businessUnitId);
			logger.Debug($"Deleting groups {string.Join(",", toBeDeleted)} for period {personId}");
			foreach (var groupInfo in toBeAdded)
			{
				logger.Debug($"Add group page if not existing for {groupInfo.GroupPageCode}, {groupInfo.GroupPageName}, {groupInfo.GroupCode}, {groupInfo.GroupCode}");
				_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
				{
					GroupName = groupInfo.GroupName,
					GroupCode = groupInfo.GroupCode,
					BusinessUnitCode = businessUnitId,
					GroupIsCustom = groupInfo.IsCustom,
					GroupPageCode = groupInfo.GroupPageCode,
					GroupPageName = groupInfo.GroupPageName,
					GroupPageNameResourceKey = groupInfo.GroupPageNameResourceKey
				});
			}
			logger.Debug($"Adding groups {string.Join(",", toBeAdded)} for period {personId}");
			_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePersonForPersonPeriod(personId, toBeAdded.Select(x => x.GroupCode).ToList(), businessUnitId);
			return toBeDeleted;
		}

		// Removes any GroupPages in specified set that have no people mapped to them
		private void clearEmptyGroups(IEnumerable<Guid> toBeDeleted, Guid businessUnitId)
		{
			var groupsToDelete = (from groupId in toBeDeleted
				let peopleInGroup = _analyticsBridgeGroupPagePersonRepository.GetBridgeGroupPagePerson(groupId, businessUnitId)
				where !peopleInGroup.Any()
				select groupId).ToList();

			logger.Debug($"Clearing empty group for {string.Join(",", groupsToDelete)}");
			_analyticsGroupPageRepository.DeleteGroupPagesByGroupCodes(groupsToDelete, businessUnitId);
		}

		private static AnalyticsGroupPage getGroupPage(IEnumerable<AnalyticsGroupPage> groupPages, string resourceKey, string groupPageName)
		{
			return groupPages.FirstOrDefault(x => x.GroupPageNameResourceKey == resourceKey) ??
				   new AnalyticsGroupPage
				   {
					   GroupPageCode = Guid.NewGuid(),
					   GroupPageNameResourceKey = resourceKey,
					   GroupPageName = groupPageName
				   };
		}

		private static void handleSkills(IPersonPeriod personPeriod, List<analyticsGroupForPerson> groupIds, IEnumerable<AnalyticsGroupPage> groupPages)
		{
			var skillGroupPage = getGroupPage(groupPages, "Skill", Resources.Skill);
			var skills = personPeriod.PersonSkillCollection.Select(s => new analyticsGroupForPerson(s.Skill.Id.GetValueOrDefault(), s.Skill.Name, skillGroupPage));
			groupIds.AddRange(skills);
		}

		private static void handleContract(IPersonPeriod personPeriod, ICollection<analyticsGroupForPerson> groupIds, IEnumerable<AnalyticsGroupPage> groupPages)
		{
			
			var contract = personPeriod.PersonContract.Contract.Id.GetValueOrDefault();
			if (contract == Guid.Empty || !personPeriod.PersonContract.Contract.IsChoosable) return;

			var contractGroupPage = getGroupPage(groupPages, "Contracts", Resources.Contracts);
			groupIds.Add(new analyticsGroupForPerson(contract, personPeriod.PersonContract.Contract.Description.Name, contractGroupPage));
		}

		private static void handleContractSchedule(IPersonPeriod personPeriod, ICollection<analyticsGroupForPerson> groupIds, IEnumerable<AnalyticsGroupPage> groupPages)
		{
			var contractSchedule = personPeriod.PersonContract.ContractSchedule.Id.GetValueOrDefault();
			if (contractSchedule == Guid.Empty || ((IDeleteTag)personPeriod.PersonContract.ContractSchedule).IsDeleted) return;

			var contractScheduleGroupPage = getGroupPage(groupPages, "ContractSchedule", Resources.ContractSchedule);
			groupIds.Add(new analyticsGroupForPerson(contractSchedule, personPeriod.PersonContract.ContractSchedule.Description.Name, contractScheduleGroupPage));
		}

		private static void handlePartTimePercentage(IPersonPeriod personPeriod, ICollection<analyticsGroupForPerson> groupIds, IEnumerable<AnalyticsGroupPage> groupPages)
		{
			var partTimePercentage = personPeriod.PersonContract.PartTimePercentage.Id.GetValueOrDefault();
			if (partTimePercentage == Guid.Empty || !personPeriod.PersonContract.PartTimePercentage.IsChoosable) return;

			var partTimePercentageGroupPage = getGroupPage(groupPages, "PartTimepercentages", Resources.PartTimepercentages);
			groupIds.Add(new analyticsGroupForPerson(partTimePercentage, personPeriod.PersonContract.PartTimePercentage.Description.Name, partTimePercentageGroupPage));
		}

		private static void handleRuleSetBag(IPersonPeriod personPeriod, ICollection<analyticsGroupForPerson> groupIds, IEnumerable<AnalyticsGroupPage> groupPages)
		{
			if (personPeriod.RuleSetBag == null) return;

			var ruleSetBag = personPeriod.RuleSetBag.Id.GetValueOrDefault();
			if (ruleSetBag == Guid.Empty || !personPeriod.RuleSetBag.IsChoosable) return;

			var ruleSetBagGroupPage = getGroupPage(groupPages, "RuleSetBag", Resources.RuleSetBag);
			groupIds.Add(new analyticsGroupForPerson(ruleSetBag, personPeriod.RuleSetBag.Description.Name, ruleSetBagGroupPage));
		}

		private static void handleNotes(string note, ICollection<analyticsGroupForPerson> groupIds, IEnumerable<AnalyticsGroupPage> groupPages)
		{
			if (string.IsNullOrWhiteSpace(note)) return;

			var groupName = formatNoteName(note);
			var noteGroupPage = getGroupPage(groupPages, "Note", Resources.Note);
			groupIds.Add(new analyticsGroupForPerson(note.GenerateGuid(), groupName, noteGroupPage));
		}

		private static string formatNoteName(string note)
		{
			return note.Length <= 50 ? note : $"{note.Substring(0, 48)}..";
		}

		private class analyticsGroupForPerson
		{
			public analyticsGroupForPerson(Guid groupCode, string groupName, AnalyticsGroupPage groupPage)
			{
				GroupCode = groupCode;
				GroupPageNameResourceKey = groupPage.GroupPageNameResourceKey;
				GroupPageName = groupPage.GroupPageName;
				GroupPageCode = groupPage.GroupPageCode;
				GroupName = groupName;
				IsCustom = false;
			}

			public Guid GroupCode { get; private set; }
			public Guid GroupPageCode { get; private set; }
			public string GroupPageName { get; private set; }
			public string GroupPageNameResourceKey { get; private set; }
			public string GroupName { get; private set; }
			public bool IsCustom { get; set; }
		}
	}
}