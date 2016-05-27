using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{

#pragma warning disable 618
	[EnabledBy(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623, 
        Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439), DisabledBy(Toggles.PersonCollectionChanged_ToHangfire_38420)]
    public class AnalyticsPersonGroupsHandlerBus : AnalyticsPersonGroupsHandler,
        IHandleEvent<AnalyticsPersonCollectionChangedEvent>,
        IRunOnServiceBus
#pragma warning restore 618
    {
		public AnalyticsPersonGroupsHandlerBus(IPersonRepository personRepository,
			IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository,
			IAnalyticsGroupPageRepository analyticsGroupPageRepository, IGroupPageRepository groupPageRepository)
			: base(personRepository, analyticsBridgeGroupPagePersonRepository, analyticsGroupPageRepository, groupPageRepository)
		{
			
		}

        [AnalyticsUnitOfWork]
        public new virtual void Handle(AnalyticsPersonCollectionChangedEvent @event)
        {
            base.Handle(@event);
        }
    }

    [EnabledBy(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623,
        Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439, Toggles.PersonCollectionChanged_ToHangfire_38420)]
    public class AnalyticsPersonGroupsHandlerHangfire : AnalyticsPersonGroupsHandler,
        IHandleEvent<AnalyticsPersonCollectionChangedEvent>,
        IRunOnHangfire

    {
        public AnalyticsPersonGroupsHandlerHangfire(IPersonRepository personRepository,
            IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository,
            IAnalyticsGroupPageRepository analyticsGroupPageRepository, IGroupPageRepository groupPageRepository)
            : base(personRepository, analyticsBridgeGroupPagePersonRepository, analyticsGroupPageRepository, groupPageRepository)
        {

        }

        [UnitOfWork, AnalyticsUnitOfWork]
        public new virtual void Handle(AnalyticsPersonCollectionChangedEvent @event)
        {
            base.Handle(@event);
        }
    }

    public class AnalyticsPersonGroupsHandler
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsPersonGroupsHandler));
		private readonly IPersonRepository _personRepository;
		private readonly IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly IGroupPageRepository _groupPageRepository;

		public AnalyticsPersonGroupsHandler(IPersonRepository personRepository, IAnalyticsBridgeGroupPagePersonRepository analyticsBridgeGroupPagePersonRepository, IAnalyticsGroupPageRepository analyticsGroupPageRepository, IGroupPageRepository groupPageRepository)
		{
			_personRepository = personRepository;
			_analyticsBridgeGroupPagePersonRepository = analyticsBridgeGroupPagePersonRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_groupPageRepository = groupPageRepository;
		}

		public virtual void Handle(AnalyticsPersonCollectionChangedEvent @event)
		{
			logger.Debug($"Handle AnalyticsPersonCollectionChangedEvent for {@event.SerializedPeople}");
			foreach (var personId in @event.PersonIdCollection)
			{
				var person = _personRepository.Get(personId) ?? new Person();
				foreach (var personPeriod in person.PersonPeriodCollection)
				{
					var groupPages = _analyticsGroupPageRepository.GetBuildInGroupPageBase().ToList();
					var groupIds = new List<analyticsGroupForPerson>();

					handleSkills(personPeriod, groupIds, groupPages);
					handleContract(personPeriod, groupIds, groupPages);
					handleContractSchedule(personPeriod, groupIds, groupPages);
					handlePartTimePercentage(personPeriod, groupIds, groupPages);
					handleRuleSetBag(personPeriod, groupIds, groupPages);
					handleNotes(person.Note, groupIds, groupPages);
					handleCustomGroups(personId, groupIds);

					var deletedGroupIds = updatePersonGroups(personPeriod.Id.GetValueOrDefault(), groupIds, @event.LogOnBusinessUnitId);

					clearEmptyGroups(deletedGroupIds);
				}
				// Remove any group pages associated with deleted person periods or deleted persons
				var personPeriodIds = person.PersonPeriodCollection.Select(p => p.Id.GetValueOrDefault()).ToList();
				logger.Debug($"Deleting bridge group page person {personId} excluding person periods {string.Join(",", personPeriodIds)}");
				_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, personPeriodIds);
			}
		}

		private void handleCustomGroups(Guid personId, ICollection<analyticsGroupForPerson> groupIds)
		{
			var groupPages = _groupPageRepository.GetGroupPagesForPerson(personId);
			foreach (var groupPage in groupPages)
			{
				foreach (var rootGroup in groupPage.RootGroupCollection)
				{
					groupIds.Add(new analyticsGroupForPerson(rootGroup.Id.GetValueOrDefault(), rootGroup.Description.Name, new AnalyticsGroupPage
					{
						GroupPageCode = groupPage.Id.GetValueOrDefault(),
						GroupPageNameResourceKey = groupPage.DescriptionKey,
						GroupPageName = groupPage.Description.Name
					}));
				}
				
			}
		}

		// Add/Remove groups for a person period
		private IEnumerable<Guid> updatePersonGroups(Guid personPeriodId, ICollection<analyticsGroupForPerson> groups, Guid businessUnitId)
		{
			var currentGroups = _analyticsBridgeGroupPagePersonRepository.GetGroupPagesForPersonPeriod(personPeriodId).ToList();

			var toBeDeleted = currentGroups.Where(g => groups.All(t => t.GroupCode != g)).ToList();
			var toBeAdded = groups.Where(t => currentGroups.All(g => g != t.GroupCode)).ToList();
			
			_analyticsBridgeGroupPagePersonRepository.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, toBeDeleted);
			logger.Debug($"Deleting groups {string.Join(",", toBeDeleted)} for period {personPeriodId}");
			foreach (var groupInfo in toBeAdded)
			{
				logger.Debug($"Add group page if not existing for {groupInfo.GroupPageCode}, {groupInfo.GroupPageName}, {groupInfo.GroupCode}, {groupInfo.GroupCode}");
				_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup
				{
					GroupName = groupInfo.GroupName,
					GroupCode = groupInfo.GroupCode,
					BusinessUnitCode = businessUnitId,
					GroupIsCustom = false,
					GroupPageCode = groupInfo.GroupPageCode,
					GroupPageName = groupInfo.GroupPageName,
					GroupPageNameResourceKey = groupInfo.GroupPageNameResourceKey
				});
			}
			logger.Debug($"Adding groups {string.Join(",", toBeAdded)} for period {personPeriodId}");
			_analyticsBridgeGroupPagePersonRepository.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, toBeAdded.Select(x => x.GroupCode).ToList());
			return toBeDeleted;
		}

		// Removes any GroupPages in specified set that have no people mapped to them
		private void clearEmptyGroups(IEnumerable<Guid> toBeDeleted)
		{
			var groupsToDelete = (from groupId in toBeDeleted
				let peopleInGroup = _analyticsBridgeGroupPagePersonRepository.GetBridgeGroupPagePerson(groupId)
				where !peopleInGroup.Any()
				select groupId).ToList();

			logger.Debug($"Clearing empty group for {string.Join(",", groupsToDelete)}");
			_analyticsGroupPageRepository.DeleteGroupPagesByGroupCodes(groupsToDelete);
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
			if (contractSchedule == Guid.Empty || !personPeriod.PersonContract.ContractSchedule.IsChoosable) return;

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
			}

			public Guid GroupCode { get; private set; }
			public Guid GroupPageCode { get; private set; }
			public string GroupPageName { get; private set; }
			public string GroupPageNameResourceKey { get; private set; }
			public string GroupName { get; private set; }
		}
	}
}