using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class GroupPageCreator : IGroupPageCreator
	{
		private readonly IGroupPageFactory _groupPageFactory;
		
		public GroupPageCreator(IGroupPageFactory groupPageFactory)
		{
			_groupPageFactory = groupPageFactory;
		}

		private void createAndAddGroupPageForDate(IEnumerable<IPerson> allPermittedPersons, IScheduleDictionary schedules, IGroupScheduleGroupPageDataProvider groupPageDataProvider,
			GroupPageLight selectedGrouping, DateOnly date, ConcurrentDictionary<DateOnly, IGroupPage> dic)
		{
			var groupPage = createGroupPageForDate(allPermittedPersons, schedules, groupPageDataProvider, selectedGrouping, date);
			dic.GetOrAdd(date, groupPage);
		}

		public IGroupPagePerDate CreateGroupPagePerDate(IEnumerable<IPerson> allPermittedPersons, IScheduleDictionary schedules, IEnumerable<DateOnly> dates, IGroupScheduleGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping)
		{
			if (dates == null) throw new ArgumentNullException(nameof(dates));
			if (groupPageDataProvider == null) throw new ArgumentNullException(nameof(groupPageDataProvider));
	
			var concDic = new ConcurrentDictionary<DateOnly, IGroupPage>();
			foreach (var date in dates)
			{
				createAndAddGroupPageForDate(allPermittedPersons, schedules, groupPageDataProvider, selectedGrouping, date, concDic);
			}
			return new GroupPagePerDate(concDic);
		}

		private IGroupPage createGroupPageForDate(IEnumerable<IPerson> allPermittedPersons, IScheduleDictionary schedules, IGroupScheduleGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping, DateOnly dateOnly)
		{
			IGroupPage groupPage;

			IGroupPageOptions options = new GroupPageOptions(allPermittedPersons)
			{
				SelectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly),
				CurrentGroupPageName = selectedGrouping.DisplayName,
				CurrentGroupPageNameKey = selectedGrouping.Key
			};

			switch (selectedGrouping.Type)
			{
				case GroupPageType.Hierarchy:
					{
						var personGroupPage = _groupPageFactory.GetPersonsGroupPageCreator();
						groupPage = personGroupPage.CreateGroupPage(new[] { groupPageDataProvider.BusinessUnit}, options);
						break;
					}
				case GroupPageType.Contract:
					{
						var contractGroupPage = _groupPageFactory.GetContractsGroupPageCreator();
						groupPage = contractGroupPage.CreateGroupPage(groupPageDataProvider.ContractCollection, options);
						break;
					}
				case GroupPageType.ContractSchedule:
					{
						var contractScheduleGroupPage = _groupPageFactory.GetContractSchedulesGroupPageCreator();
						groupPage = contractScheduleGroupPage.CreateGroupPage(groupPageDataProvider.ContractScheduleCollection, options);
						break;
					}
				case GroupPageType.PartTimePercentage:
					{
						var partTimePercentageGroupPage = _groupPageFactory.GetPartTimePercentagesGroupPageCreator();
						groupPage = partTimePercentageGroupPage.CreateGroupPage(groupPageDataProvider.PartTimePercentageCollection, options);
						break;
					}
				case GroupPageType.Note:
					{
						var personNoteGroupPage = _groupPageFactory.GetNotesGroupPageCreator();
						groupPage = personNoteGroupPage.CreateGroupPage(null, options);
						break;
					}
				case GroupPageType.RuleSetBag:
					{
						var ruleSetBagGroupPage = _groupPageFactory.GetRuleSetBagsGroupPageCreator();
						groupPage = ruleSetBagGroupPage.CreateGroupPage(groupPageDataProvider.RuleSetBagCollection, options);
						break;
					}
				case GroupPageType.SingleAgent:
			        {
			            var singleAgentTeam = _groupPageFactory.GetSingleAgentTeamCreator();
			            groupPage= singleAgentTeam.CreateGroupPage(groupPageDataProvider.AllLoadedPersons, options);
                        break;
			        }
                default:
					{
						groupPage = null;// selectedGrouping;
						var groups = groupPageDataProvider.UserDefinedGroupings(schedules);
						foreach (var group in groups)
						{
							if (group.IdOrDescriptionKey.Equals(selectedGrouping.Key))
							{
								groupPage = group;
								break;
							}
						}

						break;
					}
			}
			return groupPage;
		}
	}
}