using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class GroupPageCreator : IGroupPageCreator
	{
		private readonly IGroupPageFactory _groupPageFactory;
		
		public GroupPageCreator(IGroupPageFactory groupPageFactory)
		{
			_groupPageFactory = groupPageFactory;
		}

		private void createAndAddGroupPageForDate(IGroupPageDataProvider groupPageDataProvider,
			GroupPageLight selectedGrouping, DateOnly date, ConcurrentDictionary<DateOnly, IGroupPage> dic)
		{
			var groupPage = createGroupPageForDate(groupPageDataProvider, selectedGrouping, date, false);
			dic.GetOrAdd(date, groupPage);
		}

		public IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping)
		{
			return CreateGroupPagePerDate(dates, groupPageDataProvider, selectedGrouping, false);
		}
		public IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping, bool useAllLoadedPersons)
		{
			if (dates == null) throw new ArgumentNullException("dates");
			if (groupPageDataProvider == null) throw new ArgumentNullException("groupPageDataProvider");
	
			var concDic = new ConcurrentDictionary<DateOnly, IGroupPage>();

			Parallel.ForEach(dates, date => createAndAddGroupPageForDate(groupPageDataProvider, selectedGrouping, date, concDic));

			return new GroupPagePerDate(concDic);
		}

		private IGroupPage createGroupPageForDate(IGroupPageDataProvider groupPageDataProvider, GroupPageLight selectedGrouping, DateOnly dateOnly, bool useAllLoadedPersons)
		{
			IGroupPage groupPage;
			var persons = groupPageDataProvider.PersonCollection;
			if (useAllLoadedPersons)
				persons = groupPageDataProvider.AllLoadedPersons;

			IGroupPageOptions options = new GroupPageOptions(persons)
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
						groupPage = personGroupPage.CreateGroupPage(groupPageDataProvider.BusinessUnitCollection, options);
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
						var groups = groupPageDataProvider.UserDefinedGroupings;
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