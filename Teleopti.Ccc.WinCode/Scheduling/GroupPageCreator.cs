using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IGroupPageCreator
	{
		IGroupPagePerDate CreateGroupPagePerDate(IScheduleViewBase currentView, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping);
		IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping);
		IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping, bool useAllLoadedPersons);
	}

	public class GroupPageCreator : IGroupPageCreator
	{
		private readonly IGroupPageFactory _groupPageFactory;
		
		public GroupPageCreator(IGroupPageFactory groupPageFactory)
		{
			_groupPageFactory = groupPageFactory;
		}
		
		public IGroupPagePerDate CreateGroupPagePerDate(IScheduleViewBase currentView, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping)
		{
			IDictionary<DateOnly, IGroupPage> dic = new Dictionary<DateOnly, IGroupPage>();
			var selectedPeriod = GetSelectedPeriod(currentView);
			foreach (var dateOnly in selectedPeriod.DayCollection())
			{
				var groupPage = createGroupPageForDate(groupPageDataProvider, selectedGrouping, dateOnly,false);
				dic.Add(dateOnly, groupPage);
			}
			return new GroupPagePerDate(dic);
		}

		public IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping)
		{
			return CreateGroupPagePerDate(dates, groupPageDataProvider, selectedGrouping, false);
		}
		public IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping, bool useAllLoadedPersons)
		{
			if (dates == null) throw new ArgumentNullException("dates");
			if (groupPageDataProvider == null) throw new ArgumentNullException("groupPageDataProvider");
			IDictionary<DateOnly, IGroupPage> dic = new Dictionary<DateOnly, IGroupPage>();

			foreach (var dateOnly in dates)
			{
				var groupPage = createGroupPageForDate(groupPageDataProvider, selectedGrouping, dateOnly, useAllLoadedPersons);
				dic.Add(dateOnly, groupPage);
			}

			return new GroupPagePerDate(dic);
		}

		private IGroupPage createGroupPageForDate(IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping, DateOnly dateOnly, bool useAllLoadedPersons)
		{
			IGroupPage groupPage;
			var persons = groupPageDataProvider.PersonCollection;
			if (useAllLoadedPersons)
				persons = groupPageDataProvider.AllLoadedPersons;

			IGroupPageOptions options = new GroupPageOptions(persons)
			{
				SelectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly),
				CurrentGroupPageName = selectedGrouping.Name,
				CurrentGroupPageNameKey = selectedGrouping.Key
			};

			switch (selectedGrouping.Key)
			{
				case "Main":
					{
						var personGroupPage = _groupPageFactory.GetPersonsGroupPageCreator();
						groupPage = personGroupPage.CreateGroupPage(groupPageDataProvider.BusinessUnitCollection, options);
						break;
					}
				case "Contracts":
					{
						var contractGroupPage = _groupPageFactory.GetContractsGroupPageCreator();
						groupPage = contractGroupPage.CreateGroupPage(groupPageDataProvider.ContractCollection, options);
						break;
					}
				case "ContractSchedule":
					{
						var contractScheduleGroupPage = _groupPageFactory.GetContractSchedulesGroupPageCreator();
						groupPage = contractScheduleGroupPage.CreateGroupPage(groupPageDataProvider.ContractScheduleCollection, options);
						break;
					}
				case "PartTimepercentages":
					{
						var partTimePercentageGroupPage = _groupPageFactory.GetPartTimePercentagesGroupPageCreator();
						groupPage = partTimePercentageGroupPage.CreateGroupPage(groupPageDataProvider.PartTimePercentageCollection, options);
						break;
					}
				case "Note":
					{
						var personNoteGroupPage = _groupPageFactory.GetNotesGroupPageCreator();
						groupPage = personNoteGroupPage.CreateGroupPage(null, options);
						break;
					}
				case "RuleSetBag":
					{
						var ruleSetBagGroupPage = _groupPageFactory.GetRuleSetBagsGroupPageCreator();
						groupPage = ruleSetBagGroupPage.CreateGroupPage(groupPageDataProvider.RuleSetBagCollection, options);
						break;
					}
                case "SingleAgentTeam":
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
		public static DateOnlyPeriod GetSelectedPeriod(IScheduleViewBase currentView)
		{
			if (currentView == null) throw new ArgumentNullException("currentView");
			var minDate = DateOnly.MaxValue;
			var maxDate = DateOnly.MinValue;
			foreach (var dateOnly in currentView.AllSelectedDates())
			{
				if (dateOnly < minDate)
					minDate = dateOnly;

				if (dateOnly > maxDate)
					maxDate = dateOnly;
			}

			return new DateOnlyPeriod(minDate, maxDate);
		}
	}

	public interface IGroupPageFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IGroupPageCreator<IBusinessUnit> GetPersonsGroupPageCreator();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IGroupPageCreator<IContract> GetContractsGroupPageCreator();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IGroupPageCreator<IContractSchedule> GetContractSchedulesGroupPageCreator();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IGroupPageCreator<IPartTimePercentage> GetPartTimePercentagesGroupPageCreator();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IGroupPageCreator<IPerson> GetNotesGroupPageCreator();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IGroupPageCreator<IRuleSetBag> GetRuleSetBagsGroupPageCreator();

	    IGroupPageCreator<IPerson> GetSingleAgentTeamCreator();
	}

	public class GroupPageFactory : IGroupPageFactory
	{
		public IGroupPageCreator<IBusinessUnit> GetPersonsGroupPageCreator()
		{
			return new PersonGroupPage();
		}

		public IGroupPageCreator<IContract> GetContractsGroupPageCreator()
		{
			return new ContractGroupPage();
		}

		public IGroupPageCreator<IContractSchedule> GetContractSchedulesGroupPageCreator()
		{
			return new ContractScheduleGroupPage();
		}

		public IGroupPageCreator<IPartTimePercentage> GetPartTimePercentagesGroupPageCreator()
		{
			return new PartTimePercentageGroupPage();
		}

		public IGroupPageCreator<IPerson> GetNotesGroupPageCreator()
		{
			return new PersonNoteGroupPage();
		}

		public IGroupPageCreator<IRuleSetBag> GetRuleSetBagsGroupPageCreator()
		{
			return new RuleSetBagGroupPage();
		}

	    public IGroupPageCreator<IPerson> GetSingleAgentTeamCreator()
	    {
	        return new SingleAgentTeamGroupPage();
	    }
	}
}