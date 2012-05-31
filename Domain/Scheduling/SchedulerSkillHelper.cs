using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class SchedulerSkillHelper
    {
        private readonly ISchedulerSkillDayHelper _schedulerSkillDayHelper;

        public SchedulerSkillHelper(ISchedulerSkillDayHelper schedulerSkillDayHelper)
        {
            _schedulerSkillDayHelper = schedulerSkillDayHelper;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
        public void CreateNonBlendSkillsFromGrouping(IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping, DateOnly dateOnly,
            INonBlendSkillFromGroupingCreator nonBlendSkillFromGroupingCreator, int demand)
        {
            IGroupPage groupPage = CreateGroupPageForDate(groupPageDataProvider, selectedGrouping, dateOnly);
            nonBlendSkillFromGroupingCreator.ProcessDate(dateOnly, groupPage);
            _schedulerSkillDayHelper.AddSkillDaysToStateHolder(ForecastSource.NonBlendSkill, demand);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public static IGroupPage CreateGroupPageForDate(IGroupPageDataProvider groupPageDataProvider, IGroupPageLight selectedGrouping, DateOnly dateOnly)
        {
            IGroupPage groupPage;
            IGroupPageOptions options = new GroupPageOptions(groupPageDataProvider.PersonCollection)
            {
                SelectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly),
                CurrentGroupPageName = selectedGrouping.Name,
                CurrentGroupPageNameKey = selectedGrouping.Key
            };

            switch (selectedGrouping.Key)
            {
                case "Main":
                    {
                        var personGroupPage = new PersonGroupPage();
                        groupPage = personGroupPage.CreateGroupPage(groupPageDataProvider.BusinessUnitCollection, options);
                        break;
                    }
                case "Contracts":
                    {
                        var contractGroupPage = new ContractGroupPage();
                        groupPage = contractGroupPage.CreateGroupPage(groupPageDataProvider.ContractCollection, options);
                        break;
                    }
                case "ContractSchedule":
                    {
                        var contractScheduleGroupPage = new ContractScheduleGroupPage();
                        groupPage = contractScheduleGroupPage.CreateGroupPage(groupPageDataProvider.ContractScheduleCollection, options);
                        break;
                    }
                case "PartTimepercentages":
                    {
                        var partTimePercentageGroupPage = new PartTimePercentageGroupPage();
                        groupPage = partTimePercentageGroupPage.CreateGroupPage(groupPageDataProvider.PartTimePercentageCollection, options);
                        break;
                    }
                case "Note":
                    {
                        var personNoteGroupPage = new PersonNoteGroupPage();
                        groupPage = personNoteGroupPage.CreateGroupPage(null, options);
                        break;
                    }
                case "RuleSetBag":
                    {
                        var ruleSetBagGroupPage = new RuleSetBagGroupPage();
                        groupPage = ruleSetBagGroupPage.CreateGroupPage(groupPageDataProvider.RuleSetBagCollection, options);
                        break;
                    }
                default:
                    {
						//TODO
                    	groupPage = null; // selectedGrouping;
                        break;
                    }
            }
            return groupPage;
        }
    }
}